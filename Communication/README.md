# Principle

To establish the communication between the simulator and the Outside word we decided to use ROS as it comes with a bunch of functions allowing easier communication
between the platforms.

<img src="images/simu_schema.PNG" alt="alt text" width="500">

As shown in the upper graph, the communicatio uses a ROS-websocket. Connecting the HMI and the Simulator will allows both of them to see the topic of its neighboor.

I also decided, strictly for my own curiosity to use a web-video-server to get the camera feedback of the smulator's car on an HMI.

# Tutorial 

The tutorial to install ROS in on the following [page](http://wiki.ros.org/hydro/Installation/Ubuntu). Be sure to take the right version coresponding with your ubuntu.
