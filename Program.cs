using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using System.Speech.Synthesis;


namespace RepCopy
{
    class Program
    {
        
        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(Keys lKey);

        static bool isRunning = true;
 
        static void Main(string[] args)
        {
            Process[] processes = Process.GetProcessesByName("RepCopy");
            if(processes.Length > 1)
            {

                Console.WriteLine("\nRepCopy already running. Close previous instances to load a new one. This instance will close in 3 seconds.");

                System.Threading.Thread.Sleep(3000);
                Environment.Exit(0);
            }

            Thread TH = new Thread(new ThreadStart(TDetect));
            TH.SetApartmentState(ApartmentState.STA);
            TH.Start();

            Thread PR = new Thread(new ThreadStart(PDetect));
            PR.SetApartmentState(ApartmentState.STA);
            PR.Start();

            Console.WriteLine("-------------------------------------------------------------------------------------------------------\n\n");
            Console.WriteLine("                   R E P C O P Y               ");
            Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
            Console.WriteLine("\nBackup reps using F8, when a game is running\n");
            Console.WriteLine("\nPressing F2 will also trigger a rep backup. There is a 2s delay after pressing F2, during which you cannot save new backups. This is to allow the game to write the new replay file.\n");
            Console.WriteLine("\nUse F9 to toggle rep copying. Use this if you are using F2/F8 outside of GTA. Rep copying will auto-enable two minutes after locking, to ensure it is not forgotten.\n");
        }

        static void TDetect()
        {
            bool bF8KeyPressInstance = false;
            bool bF2KeyPressInstance = false;
            bool bF9KeyPressInstance = false;
            bool bSARunning = false;
            bool bVCRunning = false;
            bool bUserProgramLock = false;
            DateTime unlockTime = new DateTime();

            while (isRunning)
            {
                Thread.Sleep(50);

                Process[] vcProcesses = Process.GetProcessesByName("gta-vc");
                Process[] saProcesses = Process.GetProcessesByName("gta_sa");

                if (vcProcesses.Length != 0)
                {
                    if(!bVCRunning)
                    {
                        Console.WriteLine("\nVice City running. Ready to copy reps.");
                        Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                        bVCRunning = true;
                    }
                }
                else
                {
                    if(bVCRunning)
                    {
                        Console.WriteLine("\nVice City closed. Waiting for game to reopen.");
                        Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                        bVCRunning = false;
                    }
                }

                if (saProcesses.Length != 0)
                {
                    if(!bSARunning)
                    {
                        Console.WriteLine("\nSan Andreas running. Ready to copy reps.");
                        Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                        bSARunning = true;
                    }
                }
                else
                {
                    if (bSARunning)
                    {
                        Console.WriteLine("\nSan Andreas closed. Waiting for game to reopen.");
                        Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                        bSARunning = false;
                    }
                }

                // FOR MANUAL (F8) BACKUPS
                if (GetAsyncKeyState(Keys.F8) && !bF8KeyPressInstance)
                {
                    bF8KeyPressInstance = true;

                    if (!bUserProgramLock)
                    {
                        if (bVCRunning)
                        {
                            ButtonClick.DefinePathsAndTriggerCopy("VC", BackupType.Manual);
                        }
                        if (bSARunning)
                        {
                            ButtonClick.DefinePathsAndTriggerCopy("SA", BackupType.Manual);
                        }
                    }
                }
                else if (!GetAsyncKeyState(Keys.F8))
                {
                    bF8KeyPressInstance = false;
                }

                //FOR AUTO (F2) BACKUPS
                if (GetAsyncKeyState(Keys.F2) && !bF2KeyPressInstance)
                {
                    bF2KeyPressInstance = true;

                    if (!bUserProgramLock)
                    {
                        if (bVCRunning || bSARunning)
                        {
                            SpeechSynthesizer synth = new SpeechSynthesizer();
                            synth.SetOutputToDefaultAudioDevice();
                            synth.SpeakAsync("Rep Copy Locked");
                        }

                        if (bVCRunning)
                        {
                            Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                            Console.WriteLine("\nF2 Replay Save Triggered - Backing up in 2s");
                            Thread.Sleep(2000);
                            ButtonClick.DefinePathsAndTriggerCopy("VC", BackupType.F2);
                        }
                        if (bSARunning)
                        {
                            Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                            Console.WriteLine("\nF2 Replay Save Triggered - Backing up in 2s");
                            Thread.Sleep(2000);
                            ButtonClick.DefinePathsAndTriggerCopy("SA", BackupType.F2);
                        }
                    }
                }
                else if (!GetAsyncKeyState(Keys.F2))
                {
                    bF2KeyPressInstance = false;
                }


                //TO LOCK REPCOPY AND PREVENT ACTIVITY
                if (GetAsyncKeyState(Keys.F9) && !bF9KeyPressInstance)
                {
                    bF9KeyPressInstance = true;
                    
                    if (!bUserProgramLock)
                    {
                        bUserProgramLock = true;
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SetOutputToDefaultAudioDevice();
                        synth.SpeakAsync("Rep Copy Disabled");
                        Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                        Console.WriteLine("\nRep Saving Temporarily Disabled. F9 to reenable, or waiting 2 minutes to reactivate automatically.");
                        unlockTime = DateTime.Now.AddMinutes(2);
                    }
                    else
                    {
                        bUserProgramLock = false;
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SetOutputToDefaultAudioDevice();
                        synth.SpeakAsync("Rep Copy Enabled");
                        Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                        Console.WriteLine("\nRep Saving Reenabled.");
                    }
                }
                else if (!GetAsyncKeyState(Keys.F9))
                {
                    bF9KeyPressInstance = false;
                }

                if(DateTime.Compare(DateTime.Now, unlockTime) > 0 && bUserProgramLock)
                {
                    bUserProgramLock = false;
                    SpeechSynthesizer synth = new SpeechSynthesizer();
                    synth.SetOutputToDefaultAudioDevice();
                    synth.SpeakAsync("Rep Copy Enabled");
                    Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                    Console.WriteLine("\nRep Saving Reenabled.");
                }
            }
        }

        static void PDetect()
        {
            while (isRunning)
            {
                Thread.Sleep(5000);
                SpeechSynthesizer synth = new SpeechSynthesizer();
                Process[] vcProcesses = Process.GetProcessesByName("gta-vc");
                Process[] saProcesses = Process.GetProcessesByName("gta_sa");

                if(vcProcesses.Length > 1 || saProcesses.Length > 1)
                {
                    synth.SetOutputToDefaultAudioDevice();
                    synth.SpeakAsync("Background Process");
                }
            }
        }
    }

    public enum BackupType
    {
        Manual,
        F2
    }

    public class ButtonClick
    {
        public static void DefinePathsAndTriggerCopy(string game, BackupType type)
        {
            if (game == "VC")
            {
                string vcSourcePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\GTA Vice City User Files\";
                string vcTargetPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\GTA Vice City User Files\RepCopy\";
                if (!Directory.Exists(vcTargetPath))
                {
                    Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                    Console.WriteLine("\nVice City RepCopy directory does not exist. Creating.");
                    Directory.CreateDirectory(vcTargetPath);
                }
                Copy(vcSourcePath, vcTargetPath, "VC", type);
            }
            if (game == "SA")
            {
                string saSourcePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\GTA San Andreas User Files\";
                string saTargetPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\GTA San Andreas User Files\RepCopy\";
                if (!Directory.Exists(saTargetPath))
                {
                    Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                    Console.WriteLine("\nSan Andreas RepCopy directory does not exist. Creating.");
                    Directory.CreateDirectory(saTargetPath);
                }
                Copy(saSourcePath, saTargetPath, "SA", type);
            }
        }

        public static void Copy(string sourcePath, string targetPath, string game, BackupType type)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            DateTime currentDateTime = DateTime.Now;
            String stringDateTime = currentDateTime.ToString();
            String parsedDateTime = stringDateTime.Replace(' ', '_');
            parsedDateTime = parsedDateTime.Replace('\\', '_');
            parsedDateTime = parsedDateTime.Replace(':', '-');
            parsedDateTime = parsedDateTime.Replace('/', '-');

            try
            {
                Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
                Console.WriteLine("\nAttempting to copy " + game + " replay.rep");
                string sourceFile = sourcePath + "replay.rep";

                string targetFileWithoutExtension = "";
                string targetFile = "";
               
                if (type == BackupType.Manual)
                {
                    targetFileWithoutExtension = targetPath + "Manual_replay-" + parsedDateTime.ToString();
                    targetFile = targetPath + "Manual_replay-" + parsedDateTime.ToString() + ".rep";
                }

                if (type == BackupType.F2)
                {
                    targetFileWithoutExtension = targetPath + "F2_replay-" + parsedDateTime.ToString();
                    targetFile = targetPath + "F2_replay-" + parsedDateTime.ToString() + ".rep";
                }

                int i = 1;
                while (File.Exists(targetFile))
                {
                    targetFile = targetFileWithoutExtension + " (" + i.ToString() + ").rep";
                    i++;
                }

                File.Copy(sourceFile, targetFile);

                if(type == BackupType.Manual)
                {
                    synth.SpeakAsync("Replay Saved");
                }
                else if (type == BackupType.F2)
                {
                    synth.SpeakAsync("Rep Copy Unlocked");
                }

                Console.WriteLine("\n" + game + " Replay copied: " + targetFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nUnable to Copy " + game + " file. Error : " + ex);
            }
            Console.WriteLine("\n\n-------------------------------------------------------------------------------------------------------\n");
        }
    }
}

