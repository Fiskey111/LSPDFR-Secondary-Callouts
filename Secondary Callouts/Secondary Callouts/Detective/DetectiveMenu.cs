using System;
using System.Collections.Generic;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Detective
{
    internal class DetectiveMenu
    {
        private static GameFiber _menusProcessFiber;

        private static UIMenu _mainMenu;
        private static MenuPool _menuPool;

        private static UIMenuListItem _responseOptionList;

        internal static void Main()
        {
            GameFiber.StartNew(delegate
            {
                _menusProcessFiber = new GameFiber(ProcessLoop);

                _menuPool = new MenuPool();
                _mainMenu = new UIMenu("Detective/Supervisor Interaction", "");

                _menuPool.Add(_mainMenu);

                _mainMenu.RefreshIndex();

                _menusProcessFiber.Start();

                GameFiber.Hibernate();
            });
        }

        internal static void StartMenu(IEnumerable<dynamic> optionList, string question, int offset, InteractionType interactionType, Action<string> callbackAction)
        {
            if (_mainMenu.MenuItems.Contains(_responseOptionList)) _mainMenu.MenuItems.Remove(_responseOptionList);
            _responseOptionList = new UIMenuListItem("Choose an ~g~answer~w~", "Choose the appropriate ~g~answer~w~ from the list of options", optionList);
            _mainMenu.SetMenuWidthOffset(offset - 200);
            _mainMenu.Title.Caption = $"{interactionType} Interaction";
            foreach (var item in _mainMenu.MenuItems) item.Enabled = false;
            _mainMenu.AddItem(new UIMenuItem(question, "The ~y~question~w~ asked"));
            _mainMenu.AddItem(_responseOptionList);
            _mainMenu.RefreshIndex();
            _menuPool.CloseAllMenus();
            _mainMenu.Visible = true;

            _mainMenu.OnItemSelect += (sender, selectedItem, index) =>
            {
                if (sender != _mainMenu || selectedItem != _responseOptionList) return;
                $"Selected option {_responseOptionList.SelectedItem.DisplayText}".AddLog();
                callbackAction(_responseOptionList.SelectedItem.DisplayText);
                _mainMenu.Visible = false;
            };
        }

        internal enum InteractionType { Detective, Supervisor }

        internal static void ProcessLoop()
        {
            while (true)
            {
                GameFiber.Yield();

                if (_mainMenu.Visible) _menuPool.ProcessMenus();
            }
        }
    }
}
