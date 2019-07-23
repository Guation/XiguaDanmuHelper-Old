/*
 * Created by SharpDevelop.
 * User: Daniel
 * Date: 4/28/2014
 * Time: 2:38 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Runtime.InteropServices;
using System.Text;

namespace Bililive_dm
{
    /// <summary>
    /// Description of Player.
    /// </summary>
    public class Mp3Player
    {
        //To import the dll winmn.dll which allows to play mp3 files
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, int hwndCallback);
        public string msg;


        public void Open(string file)
        {
            msg = "open: ";
            string command = "open \"" + file + "\" type MPEGVideo alias Music";
            msg += mciSendString(command, null, 0, 0);
            msg += " ";
            Play();
        }

        public void Play()
        {
            msg += "play: ";
            string command = "play Music wait";
            msg += mciSendString(command, null, 0, 0);
            msg += " ";
            Stop();
        }

        public void Stop()
        {
            msg += "stop: ";
            string command = "stop Music";
            msg += mciSendString(command, null, 0, 0);
            msg += " close: ";
            command = "close Music";
            msg += mciSendString(command, null, 0, 0);
        }
    }

}
