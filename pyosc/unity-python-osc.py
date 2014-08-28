#!/usr/bin/env python3
from OSC import OSCServer, OSCClient, OSCMessage
import sys
from time import sleep

server = OSCServer( ("localhost", 8001) )
client = OSCClient()
client.connect(("localhost", 8000))
server.timeout = 0
run = True

calibration_flag = 0
calibration_step = 0
calibration_position = [0,0]

eye_tracking_flag = False


def handle_timeout(self):
    self.timed_out = True
    
import types
server.handle_timeout = types.MethodType(handle_timeout, server)

def calibration_flag_callback(path, tags, args, source): 
    global calibration_flag
    calibration_flag = args[0]
    if calibration_flag == 1:
        global eye_tracking_flag
        eye_tracking_flag = False
        print ("start calibration!")
        
    #debug: start eye tracking process
    if calibration_flag == 2:
        global eye_tracking_flag
        eye_tracking_flag = True

def calibration_step_callback(path, tags, args, source): 
    global calibration_step 
    calibration_step = args[0]
    global calibration_position
    calibration_position = args[1:]
    print 'calibration step: %d,  ' % (calibration_step ), 'position' ,calibration_position

def quit_callback(path, tags, args, source):
    global run
    run = False

server.addMsgHandler( "/calibration/flag", calibration_flag_callback )
server.addMsgHandler( "/calibration/step", calibration_step_callback )
server.addMsgHandler( "/quit", quit_callback )


def each_frame():
    # clear timed_out flag
    server.timed_out = False
    # handle all pending requests then return
    while not server.timed_out:
        server.handle_request()

# simulate a "game engine"
while run:
    sleep(0.02)

    each_frame()

    if eye_tracking_flag:
        eye_tracking_pos = [0.1, -90.1] #debug
        client.send( OSCMessage("/eye_tracking/position", eye_tracking_pos ) )

server.close()
