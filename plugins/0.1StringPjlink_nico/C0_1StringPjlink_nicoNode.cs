#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Pjlink_nico", Category = "String", Version = "1.0")]
	#endregion PluginInfo
	public class C0_1StringPjlink_nicoNode : IPluginEvaluate
	{
		#region fields & pins
		
		//input pins
		[Input("Power status Input", DefaultString = "1", IsSingle = true)]
		public IDiffSpread<string> FInPowerStatus;
		
		[Input("information Input", DefaultString = "OK", IsSingle = true)]
		public IDiffSpread<string> FInInfo;
		
		[Input("PJlink commands", DefaultString = "", IsSingle = true)]
		public IDiffSpread<string> FInCommand;
		
		[Input("Connection enstablished", DefaultString = "", IsSingle = true)]
		public IDiffSpread<string> FInRemoteHost;
		
		//output pins
		[Output("PJlink Response")]
		public ISpread<string> FOutResponse;
		
		[Output("Get Power Status", IsBang=true)]
		public ISpread<bool> FOutPowerStatus;
		
		[Output("Get Info", IsBang=true)]
		public ISpread<bool> FOutInfo;
		
		[Output("Restart", IsBang=true)]
		public ISpread<bool> FOutRestart;
		
		[Output("Standby", IsBang=true)]
		public ISpread<bool> FOutStandby;
		
		[Output("PC Reboot", IsBang=true)]
		public ISpread<bool> FOutReboot;
		
		[Output("PC Shutdown", IsBang=true)]
		public ISpread<bool> FOutShutdown;
		
		[Output("Do Send", IsBang=true)]
		public ISpread<bool> FOutSendViaTCP;
		
		//other stuff
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		public bool bPowerStatus= false;
		public bool bInfo       = false;
		public bool bRestart    = false;
		public bool bStandby    = false;
		public bool bReboot     = false;
		public bool bShutdown   = false;		
		public bool bSendViaTCP = false;
		public string response  = "";
		
		public bool checkResponse_power()
		{	
			if( FInPowerStatus[0] == "0" || FInPowerStatus[0] == "1" || FInPowerStatus[0] == "2" || FInPowerStatus[0] == "3" || 
				FInPowerStatus[0] == "ERR3" || FInPowerStatus[0] == "ERR4")
			{
				return true;
			}
			// if response is not a standard one print, an error message and exit
			FLogger.Log( LogType.Debug, "\tResponse to 'POWR ?' request is not valid.");
			FLogger.Log( LogType.Debug, "\tValid responses are [0, 1, 2, 3, ERR3, ERR4]");
			return false;
			// TODO: make a check on string dimension
			// information text included in the response 
			// cannot be longer than 32 characters.
			//int infoLen = FInPowerStatus[0].Length;
			//string infoSubSet = FInPowerStatus[0].Substring(0, Math.Min(32, infoLen ));
			//FLogger.Log(LogType.Debug, "Info are: {0}", infoSubSet);
		}
				
		public bool checkResponse_info()
		{
			if( FInInfo[0] == "OK" || FInInfo[0] == "ERR1" || FInInfo[0] == "ERR3" || FInInfo[0] == "ERR4" )
			{
				return true;
			}
			// if response is not a standard one, print an error message and exit
			FLogger.Log( LogType.Debug, "\tResponse to 'INFO ?' request is not valid.");
			FLogger.Log( LogType.Debug, "\tValid responses are [OK, ERR1, ERR3, ERR4]");
			return false;
			// TODO: make a check on string dimension
			// information text included in the response 
			// cannot be longer than 32 characters.
			//int infoLen = FInInfo[0].Length;
			//string infoSubSet = FInInfo[0].Substring(0, Math.Min(32, infoLen ));
			//FLogger.Log(LogType.Debug, "Info are: {0}", infoSubSet)
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			// reset all the bangs
			if(bPowerStatus){
				bPowerStatus = false;
				FOutPowerStatus[0] = bPowerStatus;
			}
			if(bInfo){
				bInfo = false;
				FOutInfo[0] = bInfo;
			}
			if(bStandby){
				bStandby = false;
				FOutStandby[0] = bStandby;
			}
			if(bRestart){
				bRestart = false;
				FOutRestart[0] = bRestart;
			}
			if(bSendViaTCP){
				bSendViaTCP = false;
				FOutSendViaTCP[0] = bSendViaTCP;
			}
			if(bReboot) {
				bReboot = false;
				FOutReboot[0] = bReboot;
			}
			if(bShutdown) {
				bShutdown = false;
				FOutShutdown[0] = bShutdown;
			}
			
			// react to a new incoming connection!
			if(FInRemoteHost.IsChanged)
			{
				FLogger.Log(LogType.Debug, "\n");
				// is there a colon (is it a valid connection)?
				int index = FInRemoteHost[0].IndexOf(':');
				//FLogger.Log(LogType.Debug, "index: {0}", index);				
				if(index <0) {
					// not a valid connection
					FLogger.Log(LogType.Debug, "Disconnection or invalid connection.");
				}
				else 
				{					
					FLogger.Log(LogType.Debug, "A new Incoming Connection!");
					// this is a new valid connection!
					// So we parse the IP address and PORT
					string ip = FInRemoteHost[0].Split(':')[0];
					string port = FInRemoteHost[0].Split(':')[1];
					FLogger.Log(LogType.Debug, "\t[ip {0}, port {1}]", ip, port);
					
					
					// 2018-07-06 - sending "PJ LINK 0" is no more needed!
					/*
					// send the 'PJ LINK 0' msg
					response = "PJ LINK 0";
					FOutResponse[0] = response;	
					// tell the TCP node to send the response
					bSendViaTCP = true;
					FOutSendViaTCP[0] = bSendViaTCP;
					*/
				}				
			} //end of if(FInRemoteHost.IsChanged)
			
			
			// elaborate the PJlink commands
			if(FInCommand.IsChanged)
			{
				FLogger.Log(LogType.Debug, "\nA new PJlink command!");
				//FLogger.Log(LogType.Debug, "Received: {0};", FInCommand[0]);
				
				// check if the incoming string is longer than 0
				if(FInCommand[0].Length == 0){
					// if not, this is not a valid PJlink message.
					FLogger.Log(LogType.Debug, "\tNot a valid PJlink command");
					return;
				}
				
				// parsing the incoming message
				char header  = FInCommand[0][0]; // this should be '%'
				if(header != '%') {
					// if header is not %, probably this is not a 
					// valid PJlink command.
					FLogger.Log(LogType.Debug, "\tNot a valid PJlink command");
					return;
				}
				char version = FInCommand[0][1]; // should be 1
				string body  = FInCommand[0].Substring(2, 4).ToUpper();
				char sep     = FInCommand[0][6];
				char data    = FInCommand[0][7];
				char terminator = FInCommand[0][8];
				
				// Debugging stuff:
				//FLogger.Log(LogType.Debug, "header is: {0};", header);
				//FLogger.Log(LogType.Debug, "version is: {0};", version);
				//FLogger.Log(LogType.Debug, "body is '{0}'", body);
				//FLogger.Log(LogType.Debug, "sep is '{0}'", sep);
				//FLogger.Log(LogType.Debug, "data is '{0}'", data);
				
				
				// check the message body
				if( body == "POWR" && data == '?') {
					FLogger.Log( LogType.Debug, "\tThis is a POWR Get Command");
					if( !checkResponse_power()) { return; }
					bPowerStatus = true;
					FOutPowerStatus[0] = bPowerStatus;
					response = header.ToString() + version.ToString() + body + '=' + FInPowerStatus[0] + terminator;
					
				}
				else if( body == "INFO") {
					FLogger.Log( LogType.Debug, "\tThis is an INFO Get Command");
					if( !checkResponse_info()) { return; }
					bInfo = true;
					FOutInfo[0] = bInfo;
					response = header.ToString() + version.ToString() + body + '=' + FInInfo[0] + terminator;
				}
				else if( body == "POWR" && data == '1') {
					// Remeber for each set message we must answer back
					// 'OK' or 'ERR1', 'ERR2', 'ERR3' (here we are using 'OK')
					FLogger.Log( LogType.Debug, "\tThis is a POWR Set Command 1 (restart)");
					bRestart = true;
					FOutRestart[0] = bRestart;
					response = header.ToString() + version.ToString() + body + '=' + "OK" + terminator;
				}
				else if( body == "POWR" && data == '0') {
					FLogger.Log( LogType.Debug, "\tThis is a POWR Set Command 0 (stanby)");
					bStandby= true;
					FOutStandby[0] = bStandby;
					response = header.ToString() + version.ToString() + body + '=' + "OK" + terminator;
				}
				else if( body == "SHDW" && data == 'R') {
					FLogger.Log( LogType.Debug, "\tThis is a SHDW Set Command R (reboot)");
					bReboot = true;
					FOutReboot[0] = bReboot;
					response = header.ToString() + version.ToString() + body + '=' + "OK" + terminator;
				}
				else if( body == "SHDW" && data == 'S') {
					FLogger.Log( LogType.Debug, "\tThis is a SHDW Set Command S (shutdown)");
					bShutdown = true;
					FOutShutdown[0] = bShutdown;
					response = header.ToString() + version.ToString() + body + '=' + "OK" + terminator;
				}
				else
				{
					// no valid commands
					// do nothing 
				}				
				
				FOutResponse[0] = response;	
				// tell the TCP node to send the response
				bSendViaTCP = true;
				FOutSendViaTCP[0] = bSendViaTCP;
			} // end of if(FInCommand.IsChanged)
		} // end of Evaluate
	} // end of class
}