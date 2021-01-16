/**
 * Copyright (c) 2019-2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Simulator.Bridge;
using Simulator.Bridge.Data;
using Simulator.Utilities;
using Simulator.Sensors.UI;

#pragma warning disable CS0649

namespace Simulator.Sensors
{
    [SensorType("IMU Adjustable", new[] { typeof(ImuData) })]
    public class ImuAdjustable : SensorBase
    {
        [SensorParameter]
        public string CorrectedTopic;

        [SensorParameter]
        public string CorrectedFrame;

        uint Sequence;

        IBridge Bridge;
        IWriter<ImuData> Writer;
        IWriter<CorrectedImuData> CorrectedWriter;

        Queue<Tuple<double, float, Action>> MessageQueue =
            new Queue<Tuple<double, float, Action>>();
        bool Destroyed = false;
        bool IsFirstFixedUpdate = true;
        double LastTimestamp;
        Rigidbody RigidBody;
        ImuData data;

        /* Booleans */
        // initialized: True if the sensor has already saved its initial values in variable data
        bool initialized = false;
        // Health: True if sensor is on
        bool health = false;
        // healthWasFalse: True if health was false at any time
        bool healthWasFalse = false;
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
        // startTime : Time when the simulation start
        double startTime;

        /* Standard deviation variables */
        // The USER can give a value to this variables if activateBias
        float sAcceleration = 0;
        float sAngularVelocity = 0;

        //variable for the path
        string path_IMU = @"C:\Users\doria\Desktop\project_5siec\Sensors_file";

        /* Last values variables*/
        Vector3 LastPosition;
        Vector3 LastVelocity;
        Quaternion LastOrientation;

        /* Bias cumulation variables */
        Vector3 dAcceleration;
        Vector3 dVelocity = new Vector3(0, 0, 0);
        Vector3 dPosition = new Vector3(0, 0, 0);
        Vector3 dAngularVelocity;
        Vector3 dAttitude = new Vector3(0, 0, 0);

        public override SensorDistributionType DistributionType => SensorDistributionType.HighLoad;

        public override void OnBridgeSetup(IBridge bridge)
        {
            Bridge = bridge;
            Writer = Bridge.AddWriter<ImuData>(Topic);
            if (!string.IsNullOrEmpty(CorrectedTopic))
            {
                CorrectedWriter = Bridge.AddWriter<CorrectedImuData>(CorrectedTopic);
            }
        }

        void Start()
        {
            RigidBody = GetComponentInParent<Rigidbody>();

            //We give the value of the starting time to avoid working with the time since 1970 
            startTime = SimulatorManager.Instance.CurrentTime;

            //We affect values to all paramaters using the pythons script
            activateBias = bool.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_IMU, @"data\IMU\biais.txt")));
            activateFailure = bool.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_IMU, @"data\IMU\failure.txt")));
            failTime = double.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_IMU, @"data\IMU\sTime.txt")));
            failDuration = double.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_IMU, @"data\IMU\dTime.txt")));
            sAcceleration = float.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_IMU, @"data\IMU\var_accel.txt")));
            sAngularVelocity = float.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(path_IMU, @"data\IMU\var_angular.txt")));

            if (activateFailure)
            {
                failTime = failTime + (double)SimulatorManager.Instance.CurrentTime;
                failStopTime = failTime + failDuration;
            }

            //Creation of the data file to add the values
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_IMU, @"IMU_file\Failure\IMU_pos.txt")))
            {
            }
            //Creation of the data file to add the values
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_IMU, @"IMU_file\Failure\IMU_orientation.txt")))
            {
            }
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.Combine(path_IMU, @"IMU_file\Failure\Time.txt")))
            {
            }
            Task.Run(Publisher);
        }

        void OnDestroy()
        {
            Destroyed = true;
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

                Tuple<double, float, Action> msg = null;
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
                        msg.Item3();
                    }
                    catch
                    {
                    }
                    nextPublish = now + (long)(Stopwatch.Frequency * msg.Item2);
                    LastTimestamp = msg.Item1;
                }
            }
        }

        void FixedUpdate()
        {
            if (IsFirstFixedUpdate)
            {
                lock (MessageQueue)
                {
                    MessageQueue.Clear();
                }
                IsFirstFixedUpdate = false;
            }

            var time = SimulatorManager.Instance.CurrentTime;
            if (time < LastTimestamp)
            {
                return;
            }

            var position = transform.position;
            position.Set(position.z, -position.x, position.y);
            var velocity = transform.InverseTransformDirection(RigidBody.velocity);
            velocity.Set(velocity.z, -velocity.x, velocity.y);
            var acceleration = (velocity - LastVelocity) / Time.fixedDeltaTime;
            var localGravity = transform.InverseTransformDirection(Physics.gravity);
            var angularVelocity = RigidBody.angularVelocity;
            var orientation = transform.rotation;
            orientation.Set(-orientation.z, orientation.x, -orientation.y, orientation.w);

            if (activateFailure && time >failTime && time < failStopTime )
            {
                health = false;
                healthWasFalse = true;
            }
            else
            {
                health = true;
            }

            if (!initialized)
            {

                LastPosition = position;
                LastVelocity = velocity;
                LastOrientation = orientation;
                initialized = true;

            }
            else
            {

                if (health)
                {

                    if (healthWasFalse) position = LastPosition + velocity * Time.fixedDeltaTime;
                    LastVelocity = velocity;
                    LastPosition = position;
                    LastOrientation = orientation;

                    acceleration -= new Vector3(localGravity.z, -localGravity.x, localGravity.y);
                    angularVelocity.Set(-angularVelocity.z, angularVelocity.x, -angularVelocity.y);

                    if (activateBias)
                    {
                        dAcceleration = new Vector3(genBias(sAcceleration), genBias(sAcceleration), genBias(sAcceleration));
                        acceleration += dAcceleration;
                        dVelocity = dVelocity + dAcceleration * Time.fixedDeltaTime;
                        velocity += dVelocity;
                        dPosition = dPosition + dVelocity * Time.fixedDeltaTime;
                        position += dPosition;

                        dAngularVelocity = new Vector3(genBias(sAngularVelocity), genBias(sAngularVelocity), genBias(sAngularVelocity));
                        angularVelocity += dAngularVelocity;
                        dAttitude = dAttitude + dAngularVelocity * Time.fixedDeltaTime;
                        var attitude = dAttitude + orientation.eulerAngles;
                        orientation = Quaternion.Euler(attitude.x, attitude.y, attitude.z);
                    }

                }
                else
                {

                    position = LastPosition;
                    velocity = LastVelocity;
                    orientation = LastOrientation;
                    acceleration = new Vector3(0, 0, 0);
                    angularVelocity.Set(0, 0, 0);

                }

            }

            /*position.Set(position.z, -position.x, position.y);
            velocity.Set(velocity.z, -velocity.x, velocity.y);
            LastVelocity = velocity;
            acceleration -= new Vector3(localGravity.z, -localGravity.x, localGravity.y);
            angularVelocity.Set(-angularVelocity.z, angularVelocity.x, -angularVelocity.y); // converting to right handed xyz
            orientation.Set(-orientation.z, orientation.x, -orientation.y, orientation.w); // converting to right handed xyz*/

            data = new ImuData()
            {
                Name = Name,
                Frame = Frame,
                Time = time,
                Sequence = Sequence,

                MeasurementSpan = Time.fixedDeltaTime,

                Position = position,
                Orientation = orientation,

                Acceleration = acceleration,
                LinearVelocity = velocity,
                AngularVelocity = angularVelocity,
            };

            var correctedData = new CorrectedImuData()
            {
                Name = Name,
                Frame = CorrectedFrame,
                Time = time,
                Sequence = Sequence,

                MeasurementSpan = Time.fixedDeltaTime,

                Position = position,
                Orientation = orientation,

                Acceleration = acceleration,
                LinearVelocity = velocity,
                AngularVelocity = angularVelocity,
            };

            if (Bridge == null || Bridge.Status != Status.Connected)
            {
                return;
            }

            lock (MessageQueue)
            {
                MessageQueue.Enqueue(Tuple.Create(time, Time.fixedDeltaTime, (Action)(() => {
                    Writer.Write(data);
                    if (CorrectedWriter != null)
                    {
                        CorrectedWriter.Write(correctedData);
                    }
                })));
            }

            Sequence++;
        }

        void Update()
        {
            IsFirstFixedUpdate = true;
        }

        public override void OnVisualize(Visualizer visualizer)
        {
            UnityEngine.Debug.Assert(visualizer != null);

            if (data == null)
            {
                return;
            }

            var attitude = data.Orientation.eulerAngles;

            var graphData = new Dictionary<string, object>()
            {
                /*{"Measurement Span", data.MeasurementSpan},*/
                {"Time", data.Time-startTime},
                //{"Simuation time", data.Time-startTime},
                {"Position", data.Position},
                {"Orientation", attitude},               
                {"Acceleration", data.Acceleration},
                {"Linear Velocity", data.LinearVelocity},
                {"Angular Velocity", data.AngularVelocity}
            };
            visualizer.UpdateGraphValues(graphData);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_IMU, @"IMU_file\Failure\IMU_pos.txt"), true))
            {
                file.WriteLine((data.Position).ToString());
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_IMU, @"IMU_file\Failure\IMU_orientation.txt"), true))
            {
                file.WriteLine((attitude).ToString());
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(path_IMU, @"IMU_file\Failure\Time.txt"), true))
            {
                file.WriteLine((data.Time - startTime).ToString());
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