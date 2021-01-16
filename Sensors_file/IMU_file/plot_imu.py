# -*- coding: utf-8 -*-
"""
Created on Sat Dec  5 20:12:39 2020

@author: doria
"""

# -*- coding: utf-8 -*-
"""
Created on Sat Dec  5 17:40:06 2020

@author: doria
"""
import numpy as np
import matplotlib.pyplot as plt

#ouverture des fichiers failure################################################
position_fail = open('./Failure/IMU_pos.txt','r')
orientation_fail = open('./Failure/IMU_orientation.txt','r')
temps = open('./Failure/Time.txt','r')

#ouverture des fichiers working################################################
position_work = open('./Working/IMU_pos.txt','r')
orientation_work = open('./Working/IMU_orientation.txt','r')



#creation des vecteurs fail####################################################
pos_fail_x = []
pos_fail_y = []
pos_fail_z = []

orien_fail_x = []
orien_fail_y = []
orien_fail_z = []
time = []

#creation des vecteurs work####################################################
pos_work_x = []
pos_work_y = []
pos_work_z = []
orien_work_x = []
orien_work_y = []
orien_work_z = []

#lecture des fichiers fail#####################################################
#position###########################
line_pos_fail = position_fail.readline()

line_pos_fail= line_pos_fail.replace(',','')
line_pos_fail= line_pos_fail.replace('(','')
line_pos_fail= line_pos_fail.replace(')','')
line_pos_fail= line_pos_fail.split()

while line_pos_fail:
        pos_fail_x.append(float(line_pos_fail[0]))
        pos_fail_y.append(float(line_pos_fail[1]))
        pos_fail_z.append(float(line_pos_fail[2]))
        
        
        #on fait joli pour la conversion
        line_pos_fail = position_fail.readline()

        line_pos_fail= line_pos_fail.replace(',','')
        line_pos_fail = line_pos_fail.replace('(','')
        line_pos_fail= line_pos_fail.replace(')','')
        line_pos_fail= line_pos_fail.split()
        
# On ferme le fichier txt 
position_fail.close()

#orientation###########################
line_orien_fail = orientation_fail.readline()

line_orien_fail= line_orien_fail.replace(',','')
line_orien_fail= line_orien_fail.replace('(','')
line_orien_fail= line_orien_fail.replace(')','')
line_orien_fail= line_orien_fail.split()

while line_orien_fail:
        orien_fail_x.append(float(line_orien_fail[0]))
        orien_fail_y.append(float(line_orien_fail[1]))
        orien_fail_z.append(float(line_orien_fail[2]))
        
        
        #on fait joli pour la conversion
        line_orien_fail = orientation_fail.readline()

        line_orien_fail= line_orien_fail.replace(',','')
        line_orien_fail = line_orien_fail.replace('(','')
        line_orien_fail= line_orien_fail.replace(')','')
        line_orien_fail= line_orien_fail.split()
        
# On ferme le fichier txt 
orientation_fail.close()

#temps#############################

line_t_fail = temps.readline()
line_t_fail = line_t_fail[:-2]
line_t_fail = line_t_fail.replace(',','.')

while line_t_fail:
        time.append(float(line_t_fail))
        
        #on fait joli pour la conversion
        line_t_fail = temps.readline()
        line_t_fail = line_t_fail[:-2]
        line_t_fail = line_t_fail.replace(',','.')
        
# On ferme le fichier txt       
temps.close()

#lecture des fichiers work#####################################################
#position###########################
line_pos_work = position_work.readline()

line_pos_work= line_pos_work.replace(',','')
line_pos_work= line_pos_work.replace('(','')
line_pos_work= line_pos_work.replace(')','')
line_pos_work= line_pos_work.split()

while line_pos_work:
        pos_work_x.append(float(line_pos_work[0]))
        pos_work_y.append(float(line_pos_work[1]))
        pos_work_z.append(float(line_pos_work[2]))
        
        
        #on fait joli pour la conversion
        line_pos_work = position_work.readline()

        line_pos_work= line_pos_work.replace(',','')
        line_pos_work = line_pos_work.replace('(','')
        line_pos_work= line_pos_work.replace(')','')
        line_pos_work= line_pos_work.split()
        
# On ferme le fichier txt 
position_work.close()

#orientation###########################
line_orien_work = orientation_work.readline()

line_orien_work= line_orien_work.replace(',','')
line_orien_work= line_orien_work.replace('(','')
line_orien_work= line_orien_work.replace(')','')
line_orien_work= line_orien_work.split()

while line_orien_work:
        orien_work_x.append(float(line_orien_work[0]))
        orien_work_y.append(float(line_orien_work[1]))
        orien_work_z.append(float(line_orien_work[2]))
        
        
        #on fait joli pour la conversion
        line_orien_work = orientation_work.readline()

        line_orien_work= line_orien_work.replace(',','')
        line_orien_work = line_orien_work.replace('(','')
        line_orien_work= line_orien_work.replace(')','')
        line_orien_work= line_orien_work.split()
        
# On ferme le fichier txt 
orientation_work.close()

#conversion en vecteurs########################################################
pos_fail_x = np.array(pos_fail_x)
pos_work_x = np.array(pos_work_x)

pos_fail_y = np.array(pos_fail_y)
pos_work_y = np.array(pos_work_y)

pos_fail_z = np.array(pos_fail_z)
pos_work_z = np.array(pos_work_z)

orien_fail_x = np.array(orien_fail_x)
orien_work_x = np.array(orien_work_x)

orien_fail_y = np.array(orien_fail_y)
orien_work_y = np.array(orien_work_y)

orien_fail_z = np.array(orien_fail_z)
orien_work_z = np.array(orien_work_z)

time = np.array(time)

if len(pos_fail_x)>=len(pos_work_x):
    pos_fail_x = pos_fail_x[len(pos_fail_x)-len(pos_work_x):]
    pos_fail_y = pos_fail_y[len(pos_fail_x)-len(pos_work_x):]
    pos_fail_z = pos_fail_z[len(pos_fail_x)-len(pos_work_x):]
    pos_orien_x = pos_fail_x[len(pos_fail_x)-len(pos_work_x):]
    pos_orien_y = pos_fail_y[len(pos_fail_x)-len(pos_work_x):]
    pos_orien_z = pos_fail_z[len(pos_fail_x)-len(pos_work_x):]
    time = time[len(pos_fail_x)-len(pos_work_x):] 
else:
    pos_work_x = pos_work_x[len(pos_work_x)-len(pos_fail_x):]
    pos_work_y = pos_work_y[len(pos_work_x)-len(pos_fail_x):]
    pos_work_z = pos_work_z[len(pos_work_x)-len(pos_fail_x):]
    pos_orien_x = pos_work_x[len(pos_work_x)-len(pos_fail_x):]
    pos_orien_y = pos_work_y[len(pos_work_x)-len(pos_fail_x):]
    pos_orien_z = pos_work_z[len(pos_work_x)-len(pos_fail_x):]
    
#plot des differentes figures##################################################
plt.plot(pos_fail_x,label = "pos failure")
plt.plot(pos_work_x,label = "pos work")
plt.ylabel("position on x axis")
plt.title("position comparison on x axis")
plt.legend()
plt.show()

plt.plot(pos_fail_y,label = "pos failure")
plt.plot(pos_work_y,label = "pos work")
plt.ylabel("position on y axis")
plt.title("position comparison on y axis")
plt.legend()
plt.show()

plt.plot(pos_fail_z,label = "pos failure")
plt.plot(pos_work_z,label = "pos work")
plt.ylabel("position on z axis")
plt.title("position comparison on z axis")
plt.legend()
plt.show()

plt.plot(orien_fail_x,label = "orientation failure")
plt.plot(orien_work_x,label = "orientation work")
plt.ylabel("oientation on x axis")
plt.title("orientation comparison on x axis")
plt.legend()
plt.show()

plt.plot(orien_fail_y,label = "orientation failure")
plt.plot(orien_work_y,label = "orientation work")
plt.ylabel("orientation on y axis")
plt.title("orientation comparison on y axis")
plt.legend()
plt.show()

plt.plot(orien_fail_z,label = "orientation failure")
plt.plot(orien_work_z,label = "orientation work")
plt.ylabel("orientation on z axis")
plt.title("orientation comparison on z axis")
plt.legend()
plt.show()












