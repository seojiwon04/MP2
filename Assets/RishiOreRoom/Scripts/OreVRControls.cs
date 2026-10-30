namespace Rishi.OreRoom {
// Input routing is separate from rendering so every controller combination can be checked.
public struct OreVRButtons {
    public bool grip, trigger, alternate, build, lamp, collect, info;
}
public struct OreVRCommands {
    public bool grab, release, use, alternate, collect, lamp, menuChanged, confirm;
    public int place;
}
public enum OreVRMenu { None, Building, Information }
public sealed class OreVRControls {
    public OreVRMenu Menu { get; private set; }
    public int BuildType { get; private set; } = 3;
    public int ToolOption { get; private set; }
    public const int ToolOptionCount=6;
    OreVRButtons previous;
    bool primed;
    public void Suspend() {
        primed=false;previous=new OreVRButtons();Menu=OreVRMenu.None;
    }
    public OreVRCommands Step(OreVRButtons now) {
        var command=new OreVRCommands { place=-1 };
        // Entering the room/resuming tracking with a button held must not perform an action.
        if(!primed) { previous=now;primed=true;return command; }
        command.release=!now.grip && previous.grip;
        bool build=now.build && !previous.build, info=now.info && !previous.info;
        bool trigger=now.trigger && !previous.trigger, alternate=now.alternate && !previous.alternate;
        bool lamp=now.lamp && !previous.lamp, collect=now.collect && !previous.collect;
        if(build) {
            Menu=Menu==OreVRMenu.Building?OreVRMenu.None:OreVRMenu.Building;
            command.menuChanged=true;
        } else if(info) {
            Menu=Menu==OreVRMenu.Information?OreVRMenu.None:OreVRMenu.Information;
            command.menuChanged=true;
        } else if(Menu==OreVRMenu.Building) {
            // Dedicated buttons place each piece. Joysticks and A remain with the shared rig.
            if(trigger) command.place=3;       // Right index: Foundation
            else if(alternate) command.place=0; // Left index: Drill
            else if(lamp) command.place=1;    // X: Work Light
            else if(collect) command.place=2; // Y: Rail
            if(command.place>=0)BuildType=command.place;
        } else if(Menu==OreVRMenu.Information) {
            if(alternate)ToolOption=(ToolOption+1)%ToolOptionCount;
            else if(trigger)command.confirm=true;
        } else {
            command.grab=now.grip && !previous.grip;
            command.use=trigger;command.alternate=alternate;
            command.collect=collect;command.lamp=lamp;
        }
        previous=now;
        return command;
    }
}
}
