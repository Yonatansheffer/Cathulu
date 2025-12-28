using System.Runtime.InteropServices;

namespace RiseOfCathulu.Domains.Utilities.Player_Input.DualSense_For_Unity.Scripts
{
   public static class NativeMethods
   {
      [DllImport( "DualSenseWindowsNative" )]
      public static extern uint GetControllerCount();

      [DllImport( "DualSenseWindowsNative" )]
      public static extern ControllerInputState GetControllerInputState( uint controllerIndex );

      [DllImport( "DualSenseWindowsNative" )]
      public static extern bool SetControllerOutputState( uint controllerIndex, ControllerOutputState outputState );
   }
}