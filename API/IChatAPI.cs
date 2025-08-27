using System;
using ProjectM.Network;
using Unity.Entities;

namespace Chat.API;
 
public interface IChatAPI
{
    // Message is split to chunks of FixedString512Bytes 
    public void SendMessage(Entity userEntity, string message);
    public void SendMessageToAll(Func<User, string> getMessageForUser);
    /*
     * 
     * 1. $[White, Bold, Large] Large colored bold message
     * Will be converted to
     * <color=#fff> <-- White tag 
     *  <b> <-- Bold tag
     *      <size=20>  <-- Large tag
    *          Large colored bold message
     *      </size>
     *  </b>
     * </color>
     * 2. $[tag1, tag2, tag3]    text1$[tag4,tag5,tag6]text2 $text3 $[] text4
     * Will be converted to styled
     * "   text1text2 $text3  text"

     */
    public string ApplyCustomFormatting(string text);
}