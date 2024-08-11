﻿using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HiddenModePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new HiddenModePage { DataContext = this };
    public override string TitleKey => Keys.SystemHiddenAtMode;
    
    [Reactive] public bool IsUartEnabled { set; get; }
    [Reactive] public string TargetHeader { set; get; } = Strings.SystemWaitingForDevice;
    [Reactive] public string TargetDescription { set; get; } = Strings.PleaseWait;

    public HiddenModePageViewModel()
    {
        Loc.LanguageUpdated += OnLanguageUpdated;
        SppMessageReceiver.Instance.BaseUpdate += OnBaseStatusUpdate;
    }

    private void OnBaseStatusUpdate(object? sender, IBasicStatusUpdate e)
    {
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        var host = DeviceMessageCache.Instance.BasicStatusUpdate?.MainConnection;
        if (host == DevicesInverted.L)
        {
            TargetHeader = string.Format(Strings.HiddenModeTarget, Strings.Left);
            TargetDescription = string.Format(Strings.HiddenModeTargetRDesc);
        }
        else if (host == DevicesInverted.R)
        {
            TargetHeader = string.Format(Strings.HiddenModeTarget, Strings.Right);
            TargetDescription = string.Format(Strings.HiddenModeTargetLDesc);
        }
    }

    private void OnLanguageUpdated()
    {
        UpdateTarget();
    }

    public override void OnNavigatedTo()
    {
        BluetoothImpl.Instance.Connected += OnConnected;
        
        UpdateTarget();
        SendHiddenMode(1);
        base.OnNavigatedTo();
    }
    
    private async void OnConnected(object? sender, EventArgs e)
    {
        await Task.Delay(300);
        SendHiddenMode(1);
    }

    public override void OnNavigatedFrom()
    {
        BluetoothImpl.Instance.Connected -= OnConnected;
        
        SendHiddenMode(0);
        
        // Reconnect
        Task.Run(() =>
        {
            _ = BluetoothImpl.Instance.DisconnectAsync(true).ContinueWith(
                lastTask =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        Task.Run(() => { _ = BluetoothImpl.Instance.ConnectAsync(); });
                    });
                });
        });
        
        // TODO reconnect device here
        base.OnNavigatedFrom();
    }
    
    private async void SendHiddenMode(int mode)
    {
        /*
         * 0: Disable
         * 1: Enable for host device only
         * 2: Enable for both devices
         * 3: Enable for host device only (duplicate?)
         * 4: Enable for both devices (duplicate?)
         * 5: Enable for both devices and merge their responses into one message
         */
        Log.Debug("SendHiddenMode: Sending hidden mode: {Mode}", mode);
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.HIDDEN_CMD_MODE, (byte)mode);
    }
}

