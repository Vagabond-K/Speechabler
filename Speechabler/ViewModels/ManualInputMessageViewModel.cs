using Speechabler.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.App;
using VagabondK.Korean.Hangul;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class ManualInputMessageViewModel : NotifyPropertyChangeObject
    {
        public ManualInputMessageViewModel(SpeechUtil speechUtil, IFocusHandler focusHandler)
        {
            this.speechUtil = speechUtil;
            this.focusHandler = focusHandler;
            (hangulKeyInputModel.Automata as HangulAutomata).ReplaceInputTimeout = 600;
        }

        private readonly IFocusHandler focusHandler;
        private readonly VagabondK.Korean.ScreenInput.HangulKeyInputModel hangulKeyInputModel = new VagabondK.Korean.ScreenInput.HangulKeyInputModel();
        private readonly SpeechUtil speechUtil;

        public bool UseInputMessage { get => Get(false); set => Set(value); }
        public ManualInputMode Mode { get => Get(() => ManualInputMode.HangulKeyboard); set => Set(value); }
        public string Message { get => Get(string.Empty); set => Set(value); }
        public bool IsShift { get => Get(false); set => Set(value); }
        public bool UsePreview { get => Get(true); set => Set(value); }

        public IInstantCommand InputCharacterCommand => GetCommand<string>(arg =>
        {
            focusHandler.Focus(nameof(Message));
            hangulKeyInputModel.InputCharacterCommand.Execute(arg);
            IsShift = false;
        });

        public IInstantCommand InputBackspaceCommand => GetCommand(() =>
        {
            focusHandler.Focus(nameof(Message));
            hangulKeyInputModel.InputBackspaceCommand.Execute(null);
        });

        public IInstantCommand SpeechAndSendSmsCommand => GetCommand(() =>
        {
            speechUtil.Speech(Message);
        });

        public IInstantCommand SelectModeCommand => GetCommand<ManualInputMode?>(mode =>
        {
            if (mode != null)
                Mode = mode.Value;
        });

        public IInstantCommand EraseMessageCommand => GetCommand(() => Message = string.Empty);
        public IInstantCommand ShiftButtonCommand => GetCommand(() => IsShift = !IsShift);

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Mode):
                    IsShift = false;
                    break;
                case nameof(UseInputMessage):
                    focusHandler.Focus(nameof(Message));
                    break;
            }
        }
    }

    public enum ManualInputMode
    {
        HangulKeyboard = 0,
        Hangul3BasesPad = 1,
        EnglishKeyboard = 2,
        SpecialChracters = 3,
    }
}
