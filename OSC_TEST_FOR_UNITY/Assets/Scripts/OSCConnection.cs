	using UnityEngine;
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using UnityOSC;

	public class OSCConnection : MonoBehaviour {
		
		public int server_port;
		public int client_port;
		public string ip_address;
		public bool calibration_flag;
		public int calibration_step;
		public int calibration_max_step;
		public long last_packet_time_stamp;
		
		private Dictionary<string, ServerLog> servers;
		
		// Script initialization
		void Start() {  
			server_port = 8000;
			OSCHandler.Instance.CreateServer ("from_python", server_port);
			servers = new Dictionary<string, ServerLog>();
			//UnityEngine.Debug.Log("start menu!!");
			client_port = 8001;
			ip_address = "127.0.0.1";
			OSCHandler.Instance.CreateClient ("to_python", IPAddress.Parse(ip_address),client_port);

			calibration_flag = false;
			calibration_step = 0;
			calibration_max_step = 8;

			last_packet_time_stamp = 0;
		}
		
		void Update() {
			//receive osc message from python
			OSCHandler.Instance.UpdateLogs();
			servers = OSCHandler.Instance.Servers;
			foreach( KeyValuePair<string, ServerLog> item in servers )
			{
				if(item.Value.log.Count > 0 && item.Value.server.LocalPort == server_port) 
				{
					int lastPacketIndex = item.Value.packets.Count - 1;
					/*
					UnityEngine.Debug.Log(String.Format("SERVER: {0} ADDRESS: {1} VALUE 0: {2}", 
					                                    item.Key, // Server name
					                                    item.Value.packets[lastPacketIndex].Address, // OSC add
				                                    	item.Value.packets[lastPacketIndex].Data[0].ToString()));
					*/
					if(item.Value.packets[lastPacketIndex].TimeStamp > last_packet_time_stamp)
					{
						float pos_x = Convert.ToSingle (item.Value.packets[lastPacketIndex].Data[0]);
						float pos_y = Convert.ToSingle (item.Value.packets[lastPacketIndex].Data[1]);
						UnityEngine.Debug.Log(String.Format("eye tracing position: [{0}, {1}] {2}", pos_x, pos_y, item.Value.packets[lastPacketIndex].TimeStamp));
						last_packet_time_stamp = item.Value.packets[lastPacketIndex].TimeStamp;
					}
				}

			}
			 
			//send osc message to python
			if (Input.GetMouseButtonDown (1)) {
				Debug.Log("Send Calibration Start Flag");
				calibration_flag = true;
				calibration_step = 0;

				int calibration_start_flag = 1;
				List<object> temp = new List<object>();
				temp.Add(calibration_start_flag);
				OSCHandler.Instance.SendMessageToClient("to_python", "/calibration/flag", temp);
				}

			if (Input.GetMouseButtonDown (0)) {
				if(calibration_flag)
				{
					Debug.Log("Send Calibration Step Message");
					calibration_step++;
					List<object> temp = new List<object>();
					temp.Add(calibration_step);
					float  pos_x = 1.0F;
					float pos_y = -9.9F;
					temp.Add(pos_x);
					temp.Add(pos_y);
					OSCHandler.Instance.SendMessageToClient("to_python", "/calibration/step", temp);
				}
				if(calibration_step == calibration_max_step) calibration_step = 0;
			}
			
			//debug : check whether python side can send message to unity
			if (Input.GetKeyDown (KeyCode.S)) {
			Debug.Log("Send Eye Tracking Start Flag");
			
			int eye_tracking_start_flag = 2;
			List<object> temp = new List<object>();
			temp.Add(eye_tracking_start_flag);
			OSCHandler.Instance.SendMessageToClient("to_python", "/calibration/flag", temp);
			}
			
			//debug : stop python program
			if (Input.GetKeyDown (KeyCode.Q)) {
				Debug.Log("Stop Python Program");	
				int stop_val = 1;
				List<object> temp = new List<object>();
				temp.Add(stop_val);
				OSCHandler.Instance.SendMessageToClient("to_python", "/quit", temp);
				}
			
		}
	}