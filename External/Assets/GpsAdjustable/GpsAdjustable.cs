using Simulator.Bridge;
using Simulator.Bridge.Data;
using Simulator.Map;
using Simulator.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Simulator.Sensors.UI;

namespace Simulator.Sensors
{
    [SensorType("GPS Adjustable", new[] { typeof(GpsOdometryData) })]
    public class GpsAdjustable : SensorBase
    {
        [SensorParameter]
        [Range(1.0f, 100f)]
        public float Frequency = 12.5f;

        [SensorParameter]
        public string ChildFrame;

        [SensorParameter]
        public bool IgnoreMapOrigin = false;

        Queue<Tuple<double, Action>> MessageQueue =
            new Queue<Tuple<double, Action>>();

        bool Destroyed = false;
        bool IsFirstFixedUpdate = true;
        double LastTimestamp;

        float NextSend;
        uint SendSequence;

        IBridge Bridge;
        IWriter<GpsOdometryData> Writer;

        Rigidbody RigidBody;
        IVehicleDynamics Dynamics;
        MapOrigin MapOrigin;

        GpsOdometryData data;

        /* Booleans */
        // Health: True if sensor is on
        bool health = false;
        // activateBias: True if the USER wants to activate the bias error
        bool activateBias = false;
        // activateFailure: True if the USER wants to activate total faults 
        bool activateFailure = false;

        /* Time and duration of failure */
        // The USER can give a value to this variables if activateFailure
        // failTime: must be given in System Seconds (1970 format)
        double failTime = 0;
        // failDuration: must be given in seconds
        double failDuration = 0;
        // failStopTime: Time when sensor recovers
        double failStopTime;
        //start time
        double startTime;

        /* Standard deviation variables */
        // The USER can give a value to this variables if activateBias
        float sLatitude = 0;
        float sLongitude = 0;
        float sAltitude = 0;

        //variable for the path
        string path_GPS = @"C:\Users\doria\Desktop\project_5siec\Sensors_file";

        public override SensorDistributionType DistributionType => SensorDistributionType.LowLoad;

        private void Awake()
        {
            RigidBody = GetComponentInParent<Rigidbody>();
            Dynamics = GetComponentInParent<IVehicleDynamics>();
            MapOrigin = MapOrigin.Find();
        }

        public void Start()
        {
            //We affect values to all paramaters using the pythons script
            activateBias = bool.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\biais.txt")));
            activateFailure = bool.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\failure.txt")));
            failTime = double.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\sTime.txt")));
            failDuration = double.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\dTime.txt")));
            sLatitude = float.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\var_lat.txt")));
            sLongitude = float.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\var_long.txt")));
            sAltitude = float.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_GPS, @"data\GPS\var_alt.txt")));

            if (activateFailure)
            {
                failTime = failTime + (double)SimulatorManager.Instance.CurrentTime;
                failStopTime = failTime + failDuration;
            }
            //We give the value of the starting time to avoid working with the time since 1970 
            startTime = (double)SimulatorManager.Instance.CurrentTime;
            
            //Creation of the data file to add the values
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\GPS_lat.txt")))
            {
                //sw.WriteLine("GPS latitude");
            }
            //Creation of the data file to add the values
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\GPS_lon.txt")))
            {
                //sw.WriteLine("GPS longitude");
            }
            //Creation of the data file to add the values
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\GPS_alt.txt")))
            {
                //sw.WriteLine("GPS altitude");
            }
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\Time.txt")))
            {
                //sw.WriteLine("GPS altitude");
            }
            Task.Run(Publisher);
        }

        void OnDestroy()
        {
            Destroyed = true;
        }

        public override void OnBridgeSetup(IBridge bridge)
        {
            Bridge = bridge;
            Writer = Bridge.AddWriter<GpsOdometryData>(Topic);
        }

        void Publisher()
        {
            var nextPublish = Stopwatch.GetTimestamp();

            while (!Destroyed)
            {
                long now = Stopwatch.GetTimestamp();
                if (now < nextPublish)
                {
                    Thread.Sleep(0);
                    continue;
                }

                Tuple<double, Action> msg = null;
                lock (MessageQueue)
                {
                    if (MessageQueue.Count > 0)
                    {
                        msg = MessageQueue.Dequeue();
                    }
                }

                if (msg != null)
                {
                    try
                    {
                        msg.Item2();
                    }
                    catch
                    {
                    }
                    nextPublish = now + (long)(Stopwatch.Frequency / Frequency);
                    LastTimestamp = msg.Item1;
                }
            }
        }


        void FixedUpdate()
        {

            if (MapOrigin == null || Bridge == null || Bridge.Status != Status.Connected)
            {
                return;
            }

            if (IsFirstFixedUpdate)
            {
                lock (MessageQueue)
                {
                    MessageQueue.Clear();
                }
                IsFirstFixedUpdate = false;

                //Creation of the data file to add the values
                /*using (System.IO.StreamWriter sw = System.IO.File.CreateText(@"C:\Users\doria\Desktop\GPS_test.txt"))
                {
                    sw.WriteLine("Test");
                }*/
            }

            var time = SimulatorManager.Instance.CurrentTime;
            if (time < LastTimestamp)
            {
                return;
            }

            var location = MapOrigin.GetGpsLocation(transform.position, IgnoreMapOrigin);
            var orientation = transform.rotation;
            var angularVelocity = RigidBody.angularVelocity;
            float dLatitude = 0;
            float dLongitude = 0;
            float dAltitude = 0;

            if (activateFailure && time > failTime && time < failStopTime) health = false;
            else health = true;


            if (health)
            {
                orientation.Set(-orientation.z, orientation.x, -orientation.y, orientation.w); // converting to right handed xyz
                angularVelocity.Set(-angularVelocity.z, angularVelocity.x, -angularVelocity.y); // converting to right handed xyz

                if (activateBias)
                {
                    dLatitude = genBias(sLatitude);
                    dLongitude = genBias(sLongitude);
                    dAltitude = genBias(sAltitude);
                    location.Latitude += dLatitude;
                    location.Longitude += dLongitude;
                    location.Altitude += dAltitude;
                }
            }
            else
            {
                location.Latitude = 0;
                location.Longitude = 0;
                location.Altitude = 0;
                orientation.Set(0, 0, 0, 1); // converting to right handed xyz
                angularVelocity.Set(0, 0, 0); // converting to right handed xyz
            }
 
            data = new GpsOdometryData()
            {
                Name = Name,
                Frame = Frame,
                Time = SimulatorManager.Instance.CurrentTime,
                Sequence = SendSequence++,

                ChildFrame = ChildFrame,
                IgnoreMapOrigin = IgnoreMapOrigin,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = location.Altitude,
                Northing = location.Northing,
                Easting = location.Easting,
                Orientation = orientation,
                ForwardSpeed = Vector3.Dot(RigidBody.velocity, transform.forward),
                Velocity = RigidBody.velocity,
                AngularVelocity = angularVelocity,
                WheelAngle = Dynamics.WheelAngle,
            };

            lock (MessageQueue)
            {
                MessageQueue.Enqueue(Tuple.Create(time, (Action)(() => Writer.Write(data))));
            }


        }

        void Update()
        {
            IsFirstFixedUpdate = true;
        }

        public override void OnVisualize(Visualizer visualizer)
        {

            UnityEngine.Debug.Assert(visualizer != null);
            var time = SimulatorManager.Instance.CurrentTime;

            var location = MapOrigin.GetGpsLocation(transform.position, IgnoreMapOrigin);
            var orientation = transform.rotation;
            var angularVelocity = RigidBody.angularVelocity;
            float dLatitude = 0;
            float dLongitude = 0;
            float dAltitude = 0;

            //We affect values to all paramaters using the pythons script
            /*activateBias = bool.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\biais.txt"));
            activateFailure = bool.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\failure.txt"));
            failTime = double.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\sTime.txt"));
            failDuration = double.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\dTime.txt"));
            sLatitude = float.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\var_lat.txt"));
            sLongitude = float.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\var_long.txt"));
            sAltitude = float.Parse(System.IO.File.ReadAllText(path_GPS + @"txt\var_alt.txt"));*/

            if (activateFailure && time > failTime && time < failStopTime) health = false;
            else health = true;

            if (health)
            {
                orientation.Set(-orientation.z, orientation.x, -orientation.y, orientation.w); // converting to right handed xyz
                angularVelocity.Set(-angularVelocity.z, angularVelocity.x, -angularVelocity.y); // converting to right handed xyz

                if (activateBias)
                {
                    dLatitude = genBias(sLatitude);
                    dLongitude = genBias(sLongitude);
                    dAltitude = genBias(sAltitude);
                    location.Latitude += dLatitude;
                    location.Longitude += dLongitude;
                    location.Altitude += dAltitude;
                }
            }
            else
            {
                location.Latitude = 0;
                location.Longitude = 0;
                location.Altitude = 0;
                orientation.Set(0, 0, 0, 1); // converting to right handed xyz
                angularVelocity.Set(0, 0, 0); // converting to right handed xyz
            }

          
            // var attitude = orientation.eulerAngles;

            var graphData = new Dictionary<string, object>()
            {
                /*{"Child Frame", ChildFrame},
                {"Ignore MapOrigin", IgnoreMapOrigin},*/
                {"Time", time-startTime},
                {"Latitude", location.Latitude},
                {"Longitude", location.Longitude},
                {"Altitude", location.Altitude}
                /*,
                {"Northing", location.Northing},
                {"Easting", location.Easting},
                {"Orientation", attitude},
                {"Forward Speed", Vector3.Dot(RigidBody.velocity, transform.forward)},
                {"Velocity", RigidBody.velocity},
                {"Angular Velocity", angularVelocity}*/
            };
            visualizer.UpdateGraphValues(graphData);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\GPS_lat.txt"), true))
            {
                file.WriteLine((location.Latitude).ToString());
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\GPS_lon.txt"), true))
            {
                file.WriteLine((location.Longitude).ToString());
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\GPS_alt.txt"), true))
            {
                file.WriteLine((location.Altitude).ToString());
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_GPS, @"GPS_file\Failure\Time.txt"), true))
            {
                file.WriteLine((time - startTime).ToString());
            }
        }

        public override void OnVisualizeToggle(bool state)
        {
            //
        }

        public float genBias(float variance)
        {
            System.Random rand = new System.Random(); //reuse this if you are generating many
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         variance * randStdNormal; //random normal(mean,stdDev^2)

            return (float)randNormal;
        } 

    }
}
