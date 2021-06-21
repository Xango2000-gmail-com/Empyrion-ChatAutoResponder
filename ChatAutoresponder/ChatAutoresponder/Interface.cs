using System;
using System.Collections.Generic;
using Eleon.Modding;

namespace ChatAutoresponder
{
    public class MyEmpyrionMod : ModInterface
    {
        ModGameAPI GameAPI;
        public string ModVersion = "ChatAutoresponder v0.0.2";
        public string ModPath = "Content\\Mods\\ChatAutoresponder\\";
        private Keywords.Root Autoresponder = new Keywords.Root { };
        private Setup.Root SetupYaml = new Setup.Root { };
        private Dictionary<string, Keywords.Responder> ResponderDictionary = new Dictionary<string, Keywords.Responder> { };
        int counter = 0;

        private void LogFile(string FileName, string FileData)
        {
            if (!System.IO.File.Exists(ModPath + FileName))
            {
                System.IO.File.Create(ModPath + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText(ModPath + FileName, FileData2);
        }

        internal static string ArrayConcatenate(int start, string[] array)
        {
            string message = "";
            for (int i = start; i < array.Length; i++)
            {
                message = message + "\r\n";
                message = message + array[i];
            }
            return message;
        }

        //########################################################################################################################################################
        //################################################ This is where the actual Empyrion Modding API stuff Begins ############################################
        //########################################################################################################################################################
        public void Game_Start(ModGameAPI gameAPI)
        {
            //Triggered when the server is booting up.
            GameAPI = gameAPI;
            System.IO.File.WriteAllText(ModPath + "Debug.txt", ""); //Blanks the Debug.txt file on server start
            Autoresponder = Keywords.Retrieve(ModPath + "Keywords.yaml");
            SetupYaml = Setup.Retrieve(ModPath + "Setup.yaml");
            foreach (Keywords.Responder Item in Autoresponder.Responses)
            {
                ResponderDictionary[Item.Keyword] = Item;
            }
        }

        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_ChatMessage:
                        //Triggered when player says something in-game
                        ChatInfo Received_ChatInfo = (ChatInfo)data;
                        foreach (string item in ResponderDictionary.Keys)
                        {
                            if (Received_ChatInfo.msg.ToLower().Contains(item.ToLower()))
                            {
                                if (ResponderDictionary[item].Type == "ServerSay")
                                {
                                    try
                                    { 
                                    string Message = System.IO.File.ReadAllText(ModPath + "cannedresponses\\" + ResponderDictionary[item].Filename);
                                    string player = Convert.ToString(Received_ChatInfo.playerId);
                                    string SayThis = "say p:" + player + " '" + SetupYaml.ServerSayPrefix + Message.Replace("'", "`") + "'";
                                    GameAPI.Game_Request(CmdId.Request_ConsoleCommand, 100, new PString(SayThis));
                                    }
                                    catch
                                    {
                                        LogFile("Debug.txt", "Error in file or File not Found (" + ResponderDictionary[item].Filename + ")");
                                    }
                                    break;
                                }
                                else if (ResponderDictionary[item].Type == "TextBox")
                                {
                                    try
                                    {
                                        string[] Message = System.IO.File.ReadAllLines(ModPath + "cannedresponses\\" + ResponderDictionary[item].Filename);
                                        string Message2 = ArrayConcatenate(0, Message);
                                        string player = Convert.ToString(Received_ChatInfo.playerId);
                                        GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, 100, new DialogBoxData()
                                        {
                                            Id = Convert.ToInt32(player),
                                            MsgText = Message2,
                                            PosButtonText = SetupYaml.TextBoxClose
                                        });
                                    }
                                    catch
                                    {
                                        LogFile("Debug.txt", "Error in file or File not Found (" + ResponderDictionary[item].Filename + ")");
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                        case CmdId.Event_Error:
                        if (seqNr == 100)
                        {
                            ErrorInfo err = (ErrorInfo)data;
                            ErrorType err2 = (ErrorType)data;
                            LogFile("Debug.txt", "Event_ERROR (possibly Unrelated): " + Convert.ToString(err2) + ": " + Convert.ToString(err));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogFile("Debug.txt", "Message: " + ex.Message);
                LogFile("Debug.txt", "Data: " + ex.Data);
                LogFile("Debug.txt", "HelpLink: " + ex.HelpLink);
                LogFile("Debug.txt", "InnerException: " + ex.InnerException);
                LogFile("Debug.txt", "Source: " + ex.Source);
                LogFile("Debug.txt", "StackTrace: " + ex.StackTrace);
                LogFile("Debug.txt", "TargetSite: " + ex.TargetSite);
            }
        }
        public void Game_Update()
        {
            counter = counter++;
            if (counter > SetupYaml.ReinitTicks)
            {
                Autoresponder = Keywords.Retrieve(ModPath + "Keywords.yaml");
                counter = 0;
            }
        }
        public void Game_Exit()
        {
            //Not used in this mod but it's mandatory
        }
    }
}