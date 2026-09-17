using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Obsolete("Required an id for some class...", true)]
public class HistoryManager : MonoBehaviour
{
    public List<byte[]> previous = new List<byte[]>();
    int val;
    public void ApplyAction(byte[] command)
    {
        using (var br = new BinaryReader(new MemoryStream(command)))
        {
            switch ((Command)br.ReadByte())
            {
                case Command.None:
                    break;
                case Command.RoomCreate:
                    //string name;
                    //color color;
                    //string mapBGName;
                    Color color = Color.white;
                    for (int i = 0; i < 4; i++)
                    {
                        color[i] = br.ReadSingle();
                    }
                    var r = EditorPad.Instance.ec.CreateRoom(color);
                    r.name = br.ReadString();
                    r.mapBGName = br.ReadString();
                    r.ChangeColor(EditorPad.Instance.ec);
                    break;
                case Command.RoomDestroy:
                    //intvector2 position

                    break;
                case Command.RoomUpdate:
                    break;
                case Command.CellCreate:
                    break;
                case Command.CellDestroy:
                    break;
                case Command.IconCreate:
                    break;
                case Command.IconDestroy:
                    break;
                case Command.IconUpdate:
                    break;
                case Command.DoorCreate:
                    break;
                case Command.DoorDestroy:
                    break;
                case Command.MapResize:
                    break;
                case Command.UpdateName:
                    break;
            }
        }
    }
}
public enum Command : byte
{
    None,
    RoomCreate,
    RoomDestroy,
    RoomUpdate,

    CellCreate,
    CellDestroy,

    IconCreate,
    IconDestroy,
    IconUpdate,

    DoorCreate,
    DoorDestroy,

    MapResize,
    UpdateName,
}
public class CommandWriter : BinaryWriter
{
    MemoryStream stream;
    public CommandWriter OverrideCommandAs(Command commandNew)
    {
        byte[] a = this;
        a[0] = (byte)commandNew;
        var stream = new MemoryStream(a);
        stream.Seek(0, SeekOrigin.End);
        return new CommandWriter(stream);
    }
    public CommandWriter(Command command) : this(new MemoryStream()) => Write((byte)command);
    CommandWriter(MemoryStream stream) : base(stream) => this.stream = stream;
    public static implicit operator byte[](CommandWriter writer)
    {
        writer.stream.Flush();
        return writer.stream.ToArray();
    }
}
