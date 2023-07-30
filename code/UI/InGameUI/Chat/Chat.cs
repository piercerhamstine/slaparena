using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace SlapArena.UI;

public partial class Chat : Panel{
    public struct TextChat{
        public string Name;
        public string Message;
        public Color Color;
        public long SteamId;
    }

    private const int MaxItems = 100;
    private const float MessageLifetime = 10f;

    public static Chat current;

    private Panel Canvas {get; set;}
    private TextEntry Input {get; set;}


    private readonly Queue<TextChatEntry> entries = new();

    public bool IsOpen{
        get => HasClass("open");
        set{
            SetClass("open", value);
            if(value){
                Input.Focus();
                Input.Text = string.Empty;
                Input.Label.SetCaretPosition(0);
            }
        }
    }

    protected override void OnAfterTreeRender(bool firstTime){
        base.OnAfterTreeRender(firstTime);

        Canvas.PreferScrollToBottom = true;
        Input.AcceptsFocus = true;
        Input.AllowEmojiReplace = true;

        current = this;
    }

    public override void Tick(){
        if(Sandbox.Input.Pressed("chat")){
            Open();
        }

        Input.Placeholder = string.IsNullOrEmpty(Input.Text) ? "Enter your message..." : string.Empty;
    }

    void Open(){
        AddClass("open");
        Input.Focus();
        Canvas.TryScrollToBottom();
    }

    void Close(){
        RemoveClass("open");
        Input.Blur();
        Input.Text = string.Empty;
        Input.Label.SetCaretPosition(0);
    }
    
    void Submit(){
        Log.Info("fard");
        
        var msg = Input.Text.Trim();
        Input.Text = "";

        Close();

        if(string.IsNullOrWhiteSpace(msg)){
            return;
        }
        
        SendChat(msg);
    }

    private void AddEntry(TextChatEntry entry){
        Canvas.AddChild(entry);
        Canvas.TryScrollToBottom();

        entry.BindClass("stale", () => entry.Lifetime > MessageLifetime);

        entries.Enqueue(entry);
        if(entries.Count > MaxItems){
            entries.Dequeue().Delete();
        }
    }

    [ClientRpc]
    public static void AddChatEntry(string name, string msg, long steamId = 0){
        current?.AddEntry(new TextChatEntry{ Name = name, Message = msg, Color = Color.Random, SteamId = steamId}); 
        
        if(!Game.IsListenServer){
            Log.Info($"{name}: {msg}");
        }
    }

    [ConCmd.Server("sa_say")]
    public static void SendChat(string msg){
        if(!ConsoleSystem.Caller.IsValid()){
            return;
        }

        if(msg.Contains('\n') || msg.Contains('\r')){
            return;
        }


        Log.Info("msg");
        AddChatEntry(To.Everyone, ConsoleSystem.Caller.Name, msg, ConsoleSystem.Caller.SteamId);
    }

    private void OnChatRecieved(TextChat textChat){
        AddEntry(new TextChatEntry{ Name = textChat.Name, Message = textChat.Message, Color = textChat.Color, SteamId = textChat.SteamId});
    }
}