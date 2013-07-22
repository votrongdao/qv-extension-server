using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using System.Runtime.InteropServices;


namespace QvCapDisconnect
{
    

    public partial class Disconnect : Form
    {
        public Disconnect()
        {
            InitializeComponent();

            this.DGV_Sessions.AllowUserToAddRows = false;
            this.DGV_Sessions.AllowUserToDeleteRows = false;
            this.DGV_Sessions.ReadOnly = true;
            this.DGV_Sessions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


            this.server.SelectionChangeCommitted += new EventHandler(server_SelectionChangeCommitted);
        }

        void server_SelectionChangeCommitted(object sender, EventArgs e)
        {
            String srv = this.server.SelectedItem.ToString();

            this.DGV_Sessions.Rows.Clear();

            foreach (TSManager.Session s in TSManager.ListUsers(srv))
            {
                if (s.sessionState != TSManager.WTS_CONNECTSTATE_CLASS.WTSListen && s.sessionName != "Console" && s.userName.Length != 0)
                {
                    int j = this.DGV_Sessions.Rows.Add();
                    this.DGV_Sessions.Rows[j].Cells[0].Value = s.sessionName;
                    this.DGV_Sessions.Rows[j].Cells[1].Value = s.userName;
                    this.DGV_Sessions.Rows[j].Cells[2].Value = s.sessionId;
                    this.DGV_Sessions.Rows[j].Cells[3].Value = s.sessionState;
                }
            }


            this.DGV_Sessions.AutoResizeColumns();
        }

        private void KillButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in this.DGV_Sessions.SelectedRows)
            {
                String srv = this.server.SelectedItem.ToString();

                TSManager.LogOffUser(srv, Convert.ToInt32(r.Cells[2].Value));
            }

            this.server_SelectionChangeCommitted(sender, e);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }


    }

    class TSManager
    {
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern Int32 WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
            [MarshalAs(UnmanagedType.U4)] Int32 Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern UInt32 WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSLogoffSession(
              IntPtr hServer,
              Int32 SessionId,
              bool bWait);

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo
        }

        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
            IntPtr hServer,
            Int32 sessionId,
            WTS_INFO_CLASS wtsInfoClass,
            ref IntPtr ppBuffer,
            [MarshalAs(UnmanagedType.U4)] ref Int32 pBytesReturned
        );


        public static IntPtr OpenServer(String Name)
        {
            IntPtr server = WTSOpenServer(Name);
            return server;
        }

        public static void CloseServer(IntPtr ServerHandle)
        {
            WTSCloseServer(ServerHandle);
        }

        public static void LogOffUser(String ServerName, Int32 SessionId)
        {
            IntPtr serverHandle = IntPtr.Zero;

            try
            {
                
                serverHandle = OpenServer(ServerName);

                if (serverHandle.ToInt64() != 0)
                {
                    WTSLogoffSession(serverHandle, SessionId, true);
                }
            }
            finally
            {
                CloseServer(serverHandle);
            }

        }

        public class Session
        {
            public String sessionName;
            public String userName;
            public Int32 sessionId;
            public WTS_CONNECTSTATE_CLASS sessionState;

            public Session(String sessionName, String userName, Int32 sessionId, WTS_CONNECTSTATE_CLASS sessionState)
            {
                this.sessionName = sessionName;
                this.userName = userName;
                this.sessionId = sessionId;
                this.sessionState = sessionState;
            }


        }

        public static List<Session> ListUsers(String ServerName)
        {

            IntPtr serverHandle = IntPtr.Zero;

            List<Session> resultList = new List<Session>();
            
            try
            {
                
                serverHandle = OpenServer(ServerName);

                if (serverHandle.ToInt64() != 0)
                {
                    IntPtr SessionInfoPtr = IntPtr.Zero;

                    IntPtr userPtr = IntPtr.Zero;
                    IntPtr domainPtr = IntPtr.Zero;

                    Int32 sessionCount = 0;
                    Int32 retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref SessionInfoPtr, ref sessionCount);
                    Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                    Int32 currentSession = (int)SessionInfoPtr;
                    Int32 bytes = 0;

                    if (retVal != 0)
                    {
                        for (int i = 0; i < sessionCount; i++)
                        {
                            WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)currentSession, typeof(WTS_SESSION_INFO));
                            currentSession += dataSize;

                            WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, ref userPtr, ref bytes);
                            WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, ref domainPtr, ref bytes);

                            String domain = Marshal.PtrToStringAnsi(domainPtr);
                            String user = Marshal.PtrToStringAnsi(userPtr);

                            resultList.Add(
                                new Session(
                                    si.pWinStationName
                                    ,(domain.Length == 0 || user.Length == 0) ? "" : domain + "\\" + user
                                    ,si.SessionID
                                    ,si.State
                                )
                            );
                        }

                        WTSFreeMemory(SessionInfoPtr);
                    }
                }
            }
            finally
            {
                CloseServer(serverHandle);
            }


            return resultList;
        }
    }
}