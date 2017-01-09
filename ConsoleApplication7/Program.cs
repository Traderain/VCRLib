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

            };
            public List<WMessage> WinsockMessages = new List<WMessage>()
            {

            };
        }


        public static VCRFile VCRParse(string filename)
        {
            var VCR = new VCRFile();
            int iEvent = 0;
            using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename))))
            {
                VCR.Version = br.ReadInt32();
                for (;;)
                {
                    iEvent++;
                    ushort thread = 0;
                    var eventCode = (int)char.GetNumericValue(br.ReadChar());

                    if ((eventCode & 80) == 1)
                    {
                        thread = br.ReadUInt16();
                        eventCode &= ~0x80;
                    }
                    switch ((VCREvent)eventCode)
                    {
                        case VCREvent.VCREvent_Sys_FloatTime:
                            {
                                var Sys_FloatTime = br.ReadDouble();
                                break;
                            }
                        case VCREvent.VCREvent_recvfrom:
                            {
                                var ret = br.ReadInt32();
                                if (ret > 8192)
                                {
                                    br.ReadInt32();
                                }
                                else
                                {

                                }

                                break;
                            }
                    }

                }
            }
            return VCR;
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
