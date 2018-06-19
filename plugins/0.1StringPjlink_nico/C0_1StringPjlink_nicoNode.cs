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
		[Input("information Input", DefaultString = "working", IsSingle = true)]
		public IDiffSpread<string> FInInfo;
		
		[Input("pjlink commands", DefaultString = "", IsSingle = true)]
		public IDiffSpread<string> FInCommand;
		
		[Input("Connection enstablished", DefaultString = "", IsSingle = true)]
		public IDiffSpread<string> FInRemoteHost;
		
		[Output("OutResponse")]
		public ISpread<string> FOutResponse;
		
		[Output("OutGetInfo", IsBang=true)]
		public ISpread<bool> FOutInfo;
		
		[Output("OutRestart", IsBang=true)]
		public ISpread<bool> FOutRestart;
		
		[Output("OutStandby", IsBang=true)]
		public ISpread<bool> FOutStandby;
		
		[Output("OutReboot", IsBang=true)]
		public ISpread<bool> FOutReboot;
		
		[Output("OutShutdown", IsBang=true)]
		public ISpread<bool> FOutShutdown;
		
		[Output("doSend", IsBang=true)]
		public ISpread<bool> FOutSendViaTCP;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		public bool bInfo      = false;
		public bool bRestart   = false;
		public bool bStandby   = false;
		public bool bReboot    = false;
		public bool bShutdown  = false;		
		public bool bSendViaTCP= false;
		public string response = "";
		

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			// reset all the bangs
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
				// is there a colon?
				int index = FInRemoteHost[0].IndexOf(':');
				//FLogger.Log(LogType.Debug, "index: {0}", index);				
				if(index <0) {
					// not a valid connection
					FLogger.Log(LogType.Debug, "This is not a valid connection.");
				}
				else 
				{
					FLogger.Log(LogType.Debug, "A new Incoming Connection!");
					// this is a new valid connection!
					// So we parse the IP address and PORT
					string ip = FInRemoteHost[0].Split(':')[0];
					string port = FInRemoteHost[0].Split(':')[1];
					FLogger.Log(LogType.Debug, "\t[ip {0}, port {1}]", ip, port);
					// send the 'PJ LINK 0' msg
					response = "PJ LINK 0";
					FOutResponse[0] = response;	
					// tell the TCP node to send the response
					bSendViaTCP = true;
					FOutSendViaTCP[0] = bSendViaTCP;	
				}					
			}
			
			
			// elaborate the pjlink commands
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
				if( body == "INFO") {
					FLogger.Log( LogType.Debug, "\tThis is an 'info' Get Command");
					bInfo = true;
					FOutInfo[0] = bInfo;
					//FOutInfo[0] = false;
					// information text included in the response 
					// cannot be longer than 32 characters.
					int infoLen = FInInfo[0].Length;
					string infoSubSet = FInInfo[0].Substring(0, Math.Min(32, infoLen ));
					//FLogger.Log(LogType.Debug, "Info are: {0}", infoSubSet);
					response = header.ToString() + version.ToString() + body + '=' + infoSubSet + terminator;
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
				}
				FOutResponse[0] = response;	
				// tell the TCP node to send the response
				bSendViaTCP = true;
				FOutSendViaTCP[0] = bSendViaTCP;
			}
		}
	}
}