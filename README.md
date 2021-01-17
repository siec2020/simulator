# simulator
This repository is used to share the necessary tools to use the LGSVL simulator. We provided files allowing the creation of objects such as the car or the sensors. We also added files allowing the communication via ROS with the outer world as well as python scripts allowing the dynamic acquisition of values for the sensors.

In each subfiles, you will find a README providing more instruction and thus being on the use of ROS or LGSVL as well as tutorials to complete installation and prerequisite of each component we used for the Simulator.

The following graph gives a global idea of the operating principle of the simulator:

<img src="images/simu_schema.png" alt="graph" width="1100">

To make a sort of summary concerning the files available in this repository:
1. simulateur_lgsvl-master: you will find a tutorial concerning the installation of the LGSVL simulator 
as the executable file allowing the use of the simulator and a J-SON model for the vehicle's sensors.

2. sensors_file: you will find the python executables allowing dynamic allocation of the sensors. As well as all the necessary files for it.

3. external: you will find a file for assets and assets bundles. The assets are scripts of objects that can be used as input by Unity and the assets bundles are their built version.

4. communication: you will find the file we used to establish communication with the simulator via ROS as well as an explanation/tutorial on all of it.
