using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Windows.Input;
using WinKB = System.Windows.Input.Keyboard;
using KEA = System.Windows.Input.KeyEventArgs;

namespace BlisterUI.Input {
    public enum WM_EVENT : uint {
        IME_SETCONTEXT = 0x0281,
        INPUTLANGCHANGE = 0x51,
        GETDLGCODE = 0x87,
        IME_COMPOSITION = 0x10f,

        MOUSE_MOVE = 0x200,
        MOUSE_LBUTTONDOWN = 0x201,
        MOUSE_LBUTTONUP = 0x202,
        MOUSE_MBUTTONDOWN = 0x207,
        MOUSE_MBUTTONUP = 0x208,
        MOUSE_RBUTTONDOWN = 0x204,
        MOUSE_RBUTTONUP = 0x205,
        MOUSE_XBUTTONDOWN = 0x20B,
        MOUSE_XBUTTONUP = 0x20C,
        MOUSE_WHEEL = 0x20A,
        MOUSE_HWHEEL = 0x20E,

        KEY_DOWN = 0x100,
        KEY_UP = 0x101,
        KEY_CHAR = 0x102
    }

    /// <summary>
    /// Original Idea Obtained From:
    /// http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game
    /// By: Niko Draskovic
    /// </summary>
    public static class WMHookInput {
        // Mouse Handlers
        public static event MouseMotionHandler OnMouseMotion;
        public static event MouseButtonHandler OnMouseButton;
        public static event MouseWheelHandler OnMouseWheel;

        delegate IntPtr WndProc(IntPtr hWnd, WM_EVENT msg, IntPtr wParam, IntPtr lParam);

        static bool created;
        static IntPtr prevWndProc;
        static WndProc hookProcDelegate;
        static IntPtr hIMC;

        // Various Win32 Defines
        const int GWL_WNDPROC = -4;
        const int DLGC_WANTALLKEYS = 4;

        // Win32 DLL Imports
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, WM_EVENT Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void Initialize(GameWindow window) {
            if(created) throw new InvalidOperationException("WinHook Can Only Initialize Once");
            created = true;

            hookProcDelegate = HookProc;
            prevWndProc = (IntPtr)SetWindowLong(window.Handle, GWL_WNDPROC, (int)Marshal.GetFunctionPointerForDelegate(hookProcDelegate));

            hIMC = ImmGetContext(window.Handle);

            MouseEventDispatcher.SetToHook();
        }

        static IntPtr HookProc(IntPtr hWnd, WM_EVENT msg, IntPtr wParam, IntPtr lParam) {
            IntPtr returnCode = CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);
            switch(msg) {
                case WM_EVENT.GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;
                case WM_EVENT.IME_SETCONTEXT:
                    if(wParam.ToInt32() == 1) { ImmAssociateContext(hWnd, hIMC); }
                    break;
                case WM_EVENT.INPUTLANGCHANGE:
                    ImmAssociateContext(hWnd, hIMC);
                    returnCode = (IntPtr)1;
                    break;

                // Key Events
                case WM_EVENT.KEY_DOWN:
                    KeyboardEventDispatcher.EventInput_KeyDown(null, (Keys)wParam);
                    break;
                case WM_EVENT.KEY_UP:
                    KeyboardEventDispatcher.EventInput_KeyUp(null, (Keys)wParam);
                    break;
                case WM_EVENT.KEY_CHAR:
                    KeyboardEventDispatcher.EventInput_CharEntered(null, (char)wParam, lParam.ToInt32());
                    break;

                // Mouse Events
                case WM_EVENT.MOUSE_MOVE:
                    if(OnMouseMotion != null)
                        OnMouseMotion(null, new MouseMotionEventArgs(lParam.ToInt64()));
                    break;
                case WM_EVENT.MOUSE_LBUTTONDOWN:
                    if(OnMouseButton != null)
                        OnMouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MouseButton.Left, ButtonState.Pressed));
                    break;
                case WM_EVENT.MOUSE_LBUTTONUP:
                    if(OnMouseButton != null)
                        OnMouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MouseButton.Left, ButtonState.Released));
                    break;
                case WM_EVENT.MOUSE_MBUTTONDOWN:
                    if(OnMouseButton != null)
                        OnMouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MouseButton.Middle, ButtonState.Pressed));
                    break;
                case WM_EVENT.MOUSE_MBUTTONUP:
                    if(OnMouseButton != null)
                        OnMouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MouseButton.Middle, ButtonState.Released));
                    break;
                case WM_EVENT.MOUSE_RBUTTONDOWN:
                    if(OnMouseButton != null)
                        OnMouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MouseButton.Right, ButtonState.Pressed));
                    break;
                case WM_EVENT.MOUSE_RBUTTONUP:
                    if(OnMouseButton != null)
                        OnMouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MouseButton.Right, ButtonState.Released));
                    break;
                case WM_EVENT.MOUSE_WHEEL:
                    if(OnMouseWheel != null)
                        OnMouseWheel(null, new MouseWheelEventArgs(wParam.ToInt32()));
                    break;
            }
            return returnCode;
        }
    }
}
