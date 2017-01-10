using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


//https://github.com/LestaD/SourceEngine2007/blob/master/src_main/utils/vcrtrace/vcrtrace.cpp
namespace ConsoleApplication7
{
    public class VCRParser
    {
        enum VCREvent
        {
            VCREvent_Sys_FloatTime = 0,
            VCREvent_recvfrom,
            VCREvent_SyncToken,
            VCREvent_GetCursorPos,
            VCREvent_SetCursorPos,
            VCREvent_ScreenToClient,
            VCREvent_Cmd_Exec,
            VCREvent_CmdLine,
            VCREvent_RegOpenKeyEx,
            VCREvent_RegSetValueEx,
            VCREvent_RegQueryValueEx,
            VCREvent_RegCreateKeyEx,
            VCREvent_RegCloseKey,
            VCREvent_PeekMessage,
            VCREvent_GameMsg,
            VCREvent_GetNumberOfConsoleInputEvents,
            VCREvent_ReadConsoleInput,
            VCREvent_GetKeyState,
            VCREvent_recv,
            VCREvent_send,
            VCREvent_Generic,
            VCREvent_CreateThread,
            VCREvent_WaitForSingleObject,
            VCREvent_EnterCriticalSection,
            VCREvent_Time,
            VCREvent_LocalTime,
            VCREvent_GenericString,
            VCREvent_NUMEVENTS
        }

        struct tm
        {	
            public int inttm_sec;	
            public int inttm_min;	
            public int inttm_hour;	
            public int inttm_mday;	
            public int inttm_mon;	
            public int inttm_year;	
            public int inttm_wday;	
            public int inttm_yday;	
            public int inttm_isdst;	     
        }

        struct Point
        {
            public float x;
            public float y;
        }
        struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public uint wParam;
            public long lParam;
            public int time;
            public Point pt;
        }

        public struct INPUT_RECORD
        {
            public byte[] EventType;

           /* public struct Event
            {
                public KEY_EVENT_RECORD KeyEvent;
                public MOUSE_EVENT_RECORD MouseEvent;
                public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
                public MENU_EVENT_RECORD MenuEvent;
                public FOCUS_EVENT_RECORD FocusEvent;
            }*///Todo: Add these
        }

        struct sockaddr_in
        {
            public short sin_family;   // e.g. AF_INET
            public ushort sin_port;     // e.g. htons(3490)
            public in_addr   sin_addr;     // see struct in_addr, below
            public string sin_zero;  // zero this if you want to
        }

        struct in_addr
        {
            public ulong s_addr;  // load with inet_aton()
        }

        public class VCRFile
        {
            public struct WMessage
            {
                public int m_Message;
                public string m_pName;
            }

            public int Version;

            public List<WMessage> WMessages = new List<WMessage>()
            {
                //TODO: Add these
            };
            public List<WMessage> WinsockMessages = new List<WMessage>()
            {
                //TODO: Add these
            };
        }

        public static VCRFile VCRParse(string filename)
        {
            var VCR = new VCRFile();
            try
            {
                int iEvent = 0;
                using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename))))
                {
                    VCR.Version = br.ReadInt32();
                    for (;;)
                    {
                        iEvent++;
                        ushort thread = 0;
                        var eventCode = (int) char.GetNumericValue(br.ReadChar());

                        if ((eventCode & 80) == 1)
                        {
                            thread = br.ReadUInt16();
                            eventCode &= ~0x80;
                        }
                        switch ((VCREvent) eventCode)
                        {
                            case VCREvent.VCREvent_Sys_FloatTime:
                            {
                                var Sys_FloatTime = br.ReadDouble();
                                break;
                            }
                            case VCREvent.VCREvent_recvfrom:
                            {
                                var ret = br.ReadInt32();
                                string buf = "";
                                if (ret > 8192)
                                {
                                    br.ReadInt32();
                                }
                                else
                                {
                                    if ((int) char.GetNumericValue(br.ReadChar()) == 1)
                                    {
                                        sockaddr_in from;
                                        from.sin_family = br.ReadInt16();
                                        from.sin_port = br.ReadUInt16();
                                        from.sin_addr.s_addr = br.ReadUInt64();
                                        from.sin_zero = "\0";

                                    }
                                }
                                break;
                            }
                            case VCREvent.VCREvent_SyncToken:
                            {
                                string SyncToken = new string(br.ReadChars(br.ReadInt32()));
                                break;
                            }
                            case VCREvent.VCREvent_SetCursorPos:
                            case VCREvent.VCREvent_GetCursorPos:
                            case VCREvent.VCREvent_ScreenToClient:
                            {
                                var x = br.ReadSingle();
                                var y = br.ReadSingle();
                                break;
                            }
                            case VCREvent.VCREvent_Cmd_Exec:
                            {
                                string command = new string(br.ReadChars(br.ReadInt32()));
                                break;
                            }
                            case VCREvent.VCREvent_CmdLine:
                            {
                                var length = br.ReadInt32();
                                if (length > 8192)
                                    throw new OutOfMemoryException();
                                var command = new string(br.ReadChars(length));
                                break;
                            }
                            case VCREvent.VCREvent_RegSetValueEx:
                            {
                                long ret = br.ReadInt64();
                            }
                                break;

                            case VCREvent.VCREvent_RegQueryValueEx:
                            {
                                long ret = br.ReadInt64();
                                ulong type = br.ReadUInt64(), cbData = br.ReadUInt64();
                                char dummy;

                                while (cbData > 0)
                                {
                                    dummy = br.ReadChar();
                                    cbData--;
                                }
                            }
                                break;

                            case VCREvent.VCREvent_RegCreateKeyEx:
                            {
                                long ret = br.ReadInt64();
                            }
                                break;

                            case VCREvent.VCREvent_RegCloseKey:
                                break;

                            case VCREvent.VCREvent_PeekMessage:
                            {
                                MSG msg;
                                int valid = br.ReadInt32();
                                if (valid == 1)
                                {
                                    msg.hwnd = new IntPtr(br.ReadInt32());
                                    msg.message = br.ReadUInt32();
                                    msg.wParam = br.ReadUInt32();
                                    msg.lParam = br.ReadInt64();
                                    msg.time = br.ReadInt32();
                                    msg.pt.x = br.ReadSingle();
                                    msg.pt.y = br.ReadSingle();
                                }
                            }
                                break;

                            case VCREvent.VCREvent_GameMsg:
                            {
                                bool valid = br.ReadBoolean();
                                if (valid)
                                {
                                    uint uMsg = br.ReadUInt32();
                                    uint wParam = br.ReadUInt32();
                                    long lParam = br.ReadInt64();
                                }
                            }
                                break;

                            case VCREvent.VCREvent_GetNumberOfConsoleInputEvents:
                            {
                                char val = br.ReadChar();
                                ulong nEvents = br.ReadUInt64();
                            }
                                break;

                            case VCREvent.VCREvent_ReadConsoleInput:
                            {
                                var val = br.ReadBoolean();
                                ulong nRead;
                                INPUT_RECORD[] recs = new INPUT_RECORD[1024];
                                if (val)
                                {
                                    nRead = br.ReadUInt64();
                                    for (int i = 0; i < 1024; i++)
                                    {
                                        recs[i].EventType = br.ReadBytes((int) nRead);
                                    }
                                }
                                else
                                {
                                    nRead = 0;
                                }
                            }
                                break;

                            case VCREvent.VCREvent_GetKeyState:
                            {
                                short ret = br.ReadInt16();
                            }
                                break;

                            case VCREvent.VCREvent_recv:
                            {
                                int ret = br.ReadInt32();

                                // Get the result from our file.
                                if (ret == -1)
                                {
                                    int ErrCode = br.ReadInt32();
                                }
                                else
                                {
                                    string dummyData = new string(br.ReadChars(ret));
                                }
                            }
                                break;

                            case VCREvent.VCREvent_send:
                            {
                                int ret = br.ReadInt32();
                                // Get the result from our file.
                                if (ret == -1)
                                {
                                    int errorCode = br.ReadInt32();
                                }
                            }
                                break;

                            case VCREvent.VCREvent_Generic:
                            {
                                int nameLen = br.ReadInt32();
                                string testName = "(none)";

                                if (nameLen != 255)
                                {
                                    testName = new string(br.ReadChars(nameLen));
                                }

                                int dataLen = br.ReadInt32();
                                string tempData = new string(br.ReadChars(dataLen));
                            }
                                break;

                            case VCREvent.VCREvent_CreateThread:
                                break;

                            case VCREvent.VCREvent_WaitForSingleObject:
                            {
                                var val = br.ReadChar();
                                string status;
                                switch (val)
                                {
                                    case '1':
                                        status = ("(WAIT_OBJECT_0)\n");
                                        break;
                                    case '2':
                                        status = ("(WAIT_ABANDONED)\n");
                                        break;
                                    default:
                                        status = ("(WAIT_TIMEOUT)\n");
                                        break;
                                }
                            }
                                break;

                            case VCREvent.VCREvent_EnterCriticalSection:
                                break;

                            case VCREvent.VCREvent_LocalTime:
                            {
                                tm today;
                                today.inttm_sec = br.ReadInt32();
                                today.inttm_min = br.ReadInt32();
                                today.inttm_hour = br.ReadInt32();
                                today.inttm_mday = br.ReadInt32();
                                today.inttm_mon = br.ReadInt32();
                                today.inttm_year = br.ReadInt32();
                                today.inttm_wday = br.ReadInt32();
                                today.inttm_yday = br.ReadInt32();
                                today.inttm_isdst = br.ReadInt32();
                            }
                                break;

                            case VCREvent.VCREvent_Time:
                            {
                                long today = br.ReadInt64();
                            }
                                break;

                            case VCREvent.VCREvent_GenericString:
                            {
                                var string1 = new string(br.ReadChars(br.ReadInt32()));
                                var string2 = new string(br.ReadChars(br.ReadInt32()));
                            }
                                break;

                            default:
                            {
                                throw new Exception("***ERROR*** VCR_TraceEvent: invalid event");
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {
                return VCR;
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var filepath = "test.vcr";
            Console.Title = "Valve Command Replay (VCR) parser by Traderain";
            Console.WriteLine("Loaded file: " + filepath);
            Console.ReadKey();
            var vcrfile = VCRParser.VCRParse(filepath);
            Console.WriteLine("Version:\t"+vcrfile.Version);


        }
    }
}
