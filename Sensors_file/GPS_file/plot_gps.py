# -*- coding: utf-8 -*-
"""
Created on Sat Dec  5 17:40:06 2020

@author: doria
"""
import numpy as np
import matplotlib.pyplot as plt

#ouverture des fichiers failure################################################
altitude_fail = open('./Failure/GPS_alt.txt','r')
longitude_fail = open('./Failure/GPS_lon.txt','r')
latitude_fail = open('./Failure/GPS_lat.txt','r')
temps = open('./Failure/Time.txt','r')

#ouverture des fichiers working################################################
altitude_work = open('./Working/GPS_alt.txt','r')
longitude_work = open('./Working/GPS_lon.txt','r')
latitude_work = open('./Working/GPS_lat.txt','r')


#creation des vecteurs fail####################################################
alt_fail = []
lon_fail = []
lat_fail = []
time = []

#creation des vecteurs work####################################################
alt_work = []
lon_work = []
lat_work = []

#lecture des fichiers fail#####################################################
#altitude###########################
line_alt_fail = altitude_fail.readline()
line_alt_fail = line_alt_fail[:-2]
line_alt_fail = line_alt_fail.replace(',','.')

while line_alt_fail:
        alt_fail.append(float(line_alt_fail))
        
        #on fait joli pour la conversion
        line_alt_fail = altitude_fail.readline()
        line_alt_fail = line_alt_fail[:-2]
        line_alt_fail = line_alt_fail.replace(',','.')
        
# On ferme le fichier txt       
altitude_fail.close()

#longitude##########################
line_lon_fail = longitude_fail.readline()
line_lon_fail = line_lon_fail[:-2]
line_lon_fail = line_lon_fail.replace(',','.')

while line_lon_fail:
        lon_fail.append(float(line_lon_fail))
        
        #on fait joli pour la conversion
        line_lon_fail = longitude_fail.readline()
        line_lon_fail = line_lon_fail[:-2]
        line_lon_fail = line_lon_fail.replace(',','.')
        
# On ferme le fichier txt       
longitude_fail.close()

#latitude###########################

line_lat_fail = latitude_fail.readline()
line_lat_fail = line_lat_fail[:-2]
line_lat_fail = line_lat_fail.replace(',','.')

while line_lat_fail:
        lat_fail.append(float(line_lat_fail))
        
        #on fait joli pour la conversion
        line_lat_fail = latitude_fail.readline()
        line_lat_fail = line_lat_fail[:-2]
        line_lat_fail = line_lat_fail.replace(',','.')
        
# On ferme le fichier txt       
latitude_fail.close()


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
#altitude###########################
line_alt_work = altitude_work.readline()
line_alt_work = line_alt_work[:-2]
line_alt_work = line_alt_work.replace(',','.')

while line_alt_work:
        alt_work.append(float(line_alt_work))
        
        #on fait joli pour la conversion
        line_alt_work = altitude_work.readline()
        line_alt_work = line_alt_work[:-2]
        line_alt_work = line_alt_work.replace(',','.')
        
# On ferme le fichier txt       
altitude_work.close()

#longitude##########################
line_lon_work = longitude_work.readline()
line_lon_work = line_lon_work[:-2]
line_lon_work = line_lon_work.replace(',','.')

while line_lon_work:
        lon_work.append(float(line_lon_work))
        
        #on fait joli pour la conversion
        line_lon_work = longitude_work.readline()
        line_lon_work = line_lon_work[:-2]
        line_lon_work = line_lon_work.replace(',','.')
        
# On ferme le fichier txt       
longitude_work.close()

#latitude###########################

line_lat_work = latitude_work.readline()
line_lat_work = line_lat_work[:-2]
line_lat_work = line_lat_work.replace(',','.')

while line_lat_work:
        lat_work.append(float(line_lat_work))
        
        #on fait joli pour la conversion
        line_lat_work = latitude_work.readline()
        line_lat_work = line_lat_work[:-2]
        line_lat_work = line_lat_work.replace(',','.')
        
# On ferme le fichier txt       
latitude_work.close()

#conversion en vecteurs########################################################
alt_fail = np.array(alt_fail)
alt_work = np.array(alt_work)

lon_fail = np.array(lon_fail)
lon_work = np.array(lon_work)

lat_fail = np.array(lat_fail)
lat_work = np.array(lat_work)

time = np.array(alt_fail)

if len(alt_fail)>=len(alt_work):
    alt_fail = alt_fail[len(alt_fail)-len(alt_work):]
    lat_fail = lat_fail[len(alt_fail)-len(alt_work):]
    lon_fail = lon_fail[len(alt_fail)-len(alt_work):]
    time = time[:len(alt_work)] 
else:
    lon_work = lon_fail[len(alt_work)-len(alt_fail):]
    alt_work = alt_work[len(alt_work)-len(alt_fail):]
    lat_work = lat_work[len(alt_work)-len(alt_fail):]
    

#plot des differentes figures##################################################
plt.plot(alt_fail,label = "alt failure")
plt.plot(alt_work,label = "alt work")
plt.ylabel("alt")
plt.title("altitude comparison")
plt.legend()
plt.show()

plt.plot(lat_fail,label = "lat failure")
plt.plot(lat_work,label = "lat work")
plt.ylabel("lat")
plt.title("latitude comparison")
plt.legend()
plt.show()

plt.plot(lon_fail,label = "lon failure")
plt.plot(lon_work,label = "lon work")
plt.ylabel("lon")
plt.title("longitude comparison")
plt.legend()
plt.show()


