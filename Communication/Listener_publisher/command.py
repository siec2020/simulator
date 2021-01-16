#!/usr/bin/env python
# Software License Agreement (BSD License)

import rospy
import time

from std_msgs.msg import UInt8
from std_msgs.msg import String
from geometry_msgs.msg import Twist 
from std_msgs.msg import Float32MultiArray
from std_msgs.msg import MultiArrayDimension
from autoware_msgs.msg import VehicleCmd

shift = False
previous = 0
pub = rospy.Publisher('/vehicle_cmd', VehicleCmd, queue_size=10)


def callback_motor_cmd(data):
    global shift
    global previous
    #time.sleep(8)
    
    msg = VehicleCmd()
    
    msg.header.stamp = rospy.Time.now()
    msg.header.seq += 1
    '''msg.accel_cmd.accel = 1
    msg.accel_cmd.header = msg.header
    msg.steer_cmd.steer = 10
    msg.steer_cmd.header = msg.header
    msg.gear_cmd.gear = 4
   
    
    msg.ctrl_cmd.linear_velocity = 1 #data.linear.x
    msg.ctrl_cmd.steering_angle = data.angular.z'''
    if ((previous>=0 and data.linear.x<0)or(previous<0 and data.linear.x>=0)):
    	shift = False
    	
    if (data.linear.x >= 0):
	    msg.twist_cmd.twist.linear.x = data.linear.x
	    #msg.accel_cmd.accel = 1
	    #msg.ctrl_cmd.linear_velocity = 1
	    if (not shift):
	    	print('devannnnnnnnnnnnnnnnnnnnnnnt')
	    	msg.gear = 64
	    	shift = True
    else:
	    msg.twist_cmd.twist.linear.x = -(data.linear.x)
	    #msg.accel_cmd.accel = 1
	    #msg.brake_cmd.brake = -int((data.linear.x))
	    #msg.ctrl_cmd.linear_velocity = 1
	    if (not shift):
	    	msg.gear = 2
	    	print('reverssssssssssssssssssssssse')
	    	shift = True

	    
    print (shift)
    '''msg.twist_cmd.twist.linear.y = 2
    msg.twist_cmd.twist.linear.z = 3
    msg.twist_cmd.twist.angular.x = 1
    msg.twist_cmd.twist.angular.y = 2'''
    msg.twist_cmd.twist.angular.z = (-data.angular.z+50)/50
    msg.twist_cmd.header = msg.header
    msg.brake_cmd.header = msg.header
    msg.accel_cmd.header = msg.header
    #msg.ctrl_cmd.header = msg.header
    #msg.mode = 0
    previous = data.linear.x
    pub.publish(msg)
    
    

def listener():
    rospy.init_node('listener')
    print('coucou1')
    rospy.Subscriber('/cmd_vel', Twist, callback_motor_cmd)

            

            

if __name__ == '__main__':
    
    print('coucou')
    listener()

    rospy.spin()

