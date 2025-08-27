using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Chat.API;
using Chat.Utils;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace Chat.Core;

public interface IStylingTagFormatter
{

    public bool IsKnownTag(string tag);
    public string Apply(string tag, string input);
}

public class ColorFormatter : IStylingTagFormatter
{
    public HashSet<string> KnownTags;
    public ColorFormatter()
    {
        KnownTags = typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name).ToHashSet();
    }

    public bool IsKnownTag(string tag)
    {
        return KnownTags.Any(x=>x.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }
    public string Apply(string tag, string input)
    {
        var field = typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(x => x.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            throw new Exception($"Couldn't resolve Color tag \"{tag}\" to format the following input \"{input}\"");
        }

        return input.Color((string)field.GetValue(null)!);
    }
}

public class SizeFormatter : IStylingTagFormatter
{
    public HashSet<string> KnownTags;
    public SizeFormatter()
    {
        KnownTags = typeof(Size).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name).ToHashSet();
    }
    public bool IsKnownTag(string tag)
    {
       return KnownTags.Any(x=>x.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }
    public string Apply(string tag, string input)
    {
        var field = typeof(Size).GetFields(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(x => x.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            throw new Exception($"Couldn't resolve Size tag \"{tag}\" to format the following input \"{input}\"");
        }

        return input.Size((int)field.GetValue(null)!);
    }
}
public class DecorFormatter : IStylingTagFormatter
{
    public HashSet<string> Bold;
    public HashSet<string> Underline;
    public HashSet<string> Italic;
    public DecorFormatter()
    {
        Bold = new();
        Bold.Add("Bold");
        Bold.Add("b");

        Underline = new();
        Underline.Add("Underline");
        Underline.Add("u");

        Italic = new();
        Italic.Add("Italic");
        Italic.Add("i");
    }
    public bool IsKnownTag(string tag)
    {
        return 
            Bold.Any(x=>x.Equals(tag, StringComparison.OrdinalIgnoreCase)) ||
            Underline.Any(x=>x.Equals(tag, StringComparison.OrdinalIgnoreCase)) ||
            Italic.Any(x=>x.Equals(tag, StringComparison.OrdinalIgnoreCase))
            ;
    }
    public string Apply(string tag, string input)
    {

        if (Bold.Any(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)))
        {
            return input.Bold();
        }
        
        if (Underline.Any(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)))
        {
            return input.Underline();
        }
        
        if (Italic.Any(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)))
        {
            return input.Italic();
        }
        
        
        throw new Exception($"Couldn't resolve Decoration tag \"{tag}\" to format the following input \"{input}\"");
    }
}
public class ChatAPI : IChatAPI
{
    public IStylingTagFormatter[] Formatters =
    {
        new ColorFormatter(),
        new SizeFormatter(),
        new DecorFormatter()
    };

    public string[] SplitMessageToChunks(string message)
    {
        Log.Debug($"SplitMessageToChunks Message {message} | ByteCount {Encoding.UTF8.GetByteCount(message)}");
        var chunks = Encoding.UTF8.GetByteCount(message) > 512 ? message.SplitNicely().ToArray() : new []{ message };
        return chunks;
    }
    
    public void SendMessage(Entity userEntity, string message)
    {
        var chunks = SplitMessageToChunks(message);
        if (!VCore.EntityManager.TryGetComponentData<User>(userEntity, out User user))
        {
            Log.Error($"[SendMessage] User Entity {userEntity.Index} had no User component attached to it.");
            return;
        }

        foreach (var chunk in chunks)
        {
            var chunkMessage = new FixedString512Bytes(chunk);
            ServerChatUtils.SendSystemMessageToClient(VCore.EntityManager, user, ref chunkMessage);
        }
    }
    
    public void SendMessageToAll(Func<User, string> getMessageForUser)
    {
        var connected = VCore.GetConnectedPlayerEntities();
        var server = VCore.Server.GetExistingSystemManaged<ServerBootstrapSystem>();
        foreach (var user in connected)
        {

            if (VCore.EntityManager.TryGetComponentData<User>(user, out User userComp))
            {
                SendMessage(user, getMessageForUser(userComp));
            }
        } 
    }
    
    public string ApplyCustomFormatting(string input)
    {
        //very $[complexTag] thing $[] goes $[hereTag]empty$[alsoTag] end
        
        const string regex = @"(?:\$\[([^\]]*)\])?([\s\S]+?)(?=\$\[|$)";
        var matches = Regex.Matches(input, regex);
        StringBuilder builder = new();
        foreach (Match match in matches)
        {
            
            string[] tags = match.Groups.Count > 1
                ? match.Groups[1].Value.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : new string[]{};
            var text = match.Groups[2].Value;
            // optional ux thing to ignore first whitespace after tags block like:
            // $[tags] text == $[tags]text, but != $[tags]  text
            if (tags.Length > 0 && text.StartsWith(" "))
            {
                text = text.Substring(1);
            }
            
            foreach (var tag in tags)
            {
                var formatters = Formatters.Where(x => x.IsKnownTag(tag)).ToArray();
                if (formatters.Length == 0)
                {
                    Log.Error($"Can't find text formatter for tag \"{tag}\" (input: {input})");
                    continue;
                }
                if (formatters.Length > 1)
                {
                    Log.Error($"Can't find text formatter for tag \"{tag}\" (input: {input})");
                    continue;
                }
                text = formatters.First().Apply(tag, text);
                
            }

            builder.Append(text);
        }

        return builder.ToString();
    }
}