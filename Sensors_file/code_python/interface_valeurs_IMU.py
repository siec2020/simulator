# -*- coding: utf-8 -*-
"""
Created on Sun Dec  6 16:56:25 2020

@author: doria
""" 
#***Interface graphique***
 
from tkinter import *
#creation des des fichiers textes
#Path
path_file = open("path_file_IMU.txt","r")#"C:/Users/doria/Desktop/project_5siec/Sensors_file/data/IMU/"
path = path_file.readline()
path=path.replace("\n","")
print(path)
path_file.close()

#function to get value and add it onto a text file wich will serve as imput to C# script
   
def end():
    sTime = open(path+"sTime.txt","w")
    dTime = open(path+"dTime.txt","w")
    var_accel = open (path+"var_accel.txt","w")
    var_angular = open (path+"var_angular.txt","w")
    biais = open (path+"biais.txt","w")
    failure = open (path+"failure.txt","w")
    
    sTime.write(str(startTime.get()))
    dTime.write(str(duration.get()))
    var_accel.write(str(saccel.get()))
    var_angular.write(str(sangular.get()))
    biais.write(str(var_biais.get()))
    failure.write(str(var_fail.get()))
    
    sTime.close()
    dTime.close()
    var_accel.close()
    var_angular.close()
    biais.close()
    failure.close()
    
    fenetre.destroy    
     
####création de la fenetre
fenetre = Tk()

####definition des cases et de la presentation
#liste d option
option_menu = ["true","false"]

label = Label(fenetre, text="biais actif")
label.grid(row=0,column=0)
var_biais=StringVar(fenetre)
var_biais.set(option_menu[0])
option_biais = OptionMenu(fenetre, var_biais, *option_menu)
option_biais.grid(row=0,column=2)

label = Label(fenetre, text="echec total")
label.grid(row=2,column=0)
var_fail=StringVar(fenetre)
var_fail.set(option_menu[0])
option_echec = OptionMenu(fenetre, var_fail, *option_menu)
option_echec.grid(row=2,column=2)

label = Label(fenetre, text="Variance acceleration")
label.grid(row=4,column=0) 
saccel = Entry(fenetre, width=5)
saccel.grid(row=4,column=2)

label = Label(fenetre, text="Variance angular velocity")
label.grid(row=6,column=0) 
sangular = Entry(fenetre, width=5)
sangular.grid(row=6,column=2) 


label = Label(fenetre, text="Start Time in s")
label.grid(row=10,column=0) 
startTime = Entry(fenetre, width=5)
startTime.grid(row=10,column=2) 

label = Label(fenetre, text="Duration in s")
label.grid(row=12,column=0) 
duration = Entry(fenetre, width=5)
duration.grid(row=12,column=2) 

bouton=Button(fenetre, text="Entrée", command=end)
bouton.grid(row=16,column=0)
 
fenetre.mainloop()


