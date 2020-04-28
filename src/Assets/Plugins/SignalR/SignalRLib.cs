using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR.Client;
using AOT;

public class SignalRLib
{

    private static SignalRLib instance;

    public SignalRLib()
    {
        instance = this;
    }

#if !UNITY_WEBGL || UNITY_EDITOR

    private HubConnection _connection;

    public async void Init(string hubUrl, string hubListener)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        
        _connection.On<string>(hubListener, (message) =>
        {
            OnMessageReceived(message);
        });
        
        _connection.Closed += async (exception) => { OnError(exception.Message); };
        try
        {
            await _connection.StartAsync();

            OnConnectionStarted("Connection Started");
        }
        catch (Exception ex)
        {
            OnError(ex.Message);
        }
    }

    public async void Exit()
    {
	    await _connection.StopAsync();
    }

    public async void SendMessage(string hubMethod, string hubMessage)
    {
        await _connection.InvokeAsync(hubMethod, hubMessage);
    }

#else

    [DllImport("__Internal")]
    private static extern void Connect(string url, string listener, Action<string> cnx, Action<string> msg);

    [DllImport("__Internal")]
    private static extern void Invoke(string method, string message);

    [DllImport("__Internal")]
    private static extern void Disconnect();

    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void ConnectionCallback(string message)
    {
        OnConnectionStarted(message);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void MessageCallback(string message)
    {
        OnMessageReceived(message);
    }

    public void Init(string hubUrl, string hubListener)
    {
        Connect(hubUrl, hubListener, ConnectionCallback, MessageCallback);
    }

    public void SendMessage(string hubMethod, string hubMessage)
    {
        Invoke(hubMethod, hubMessage);
    }
    
    public void Exit()
    {
        Disconnect();
    }

#endif

    public event EventHandler<MessageEventArgs> MessageReceived;
    public event EventHandler<MessageEventArgs> ConnectionStarted;
    public event EventHandler<MessageEventArgs> Error;

    private static void OnMessageReceived(string message)
    {
        var args = new MessageEventArgs();
        args.Message = message;
        instance.MessageReceived?.Invoke(instance, args);
    }

    private static void OnConnectionStarted(string message)
    {
        var args = new MessageEventArgs();
        args.Message = message;
        instance.ConnectionStarted?.Invoke(instance, args);
    }

    private static void OnError(string message)
    {
	    var args = new MessageEventArgs();
	    args.Message = message;
	    instance.Error?.Invoke(instance, args);
    }
}

public class MessageEventArgs : EventArgs
{
    public string Message { get; set; }
}
