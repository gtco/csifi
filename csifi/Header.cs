using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csifi
{
    public class Header
    {
        public const int HeaderLength = 0x040;
        public const int VersionNumber = 0x00;
        public const int Flags = 0x01;
        public const int HighMem = 0x04;
        public const int InitialPc = 0x06;
        public const int Dictionary = 0x08;
        public const int ObjectTable = 0x0A;
        public const int GlobalVar = 0x0C;
        public const int StaticMem = 0x0E;
        public const int Flags2 = 0x10;
        public const int AbbrTable = 0x18;
        public const int FileLength = 0x1A;
        public const int FileChecksum = 0x1C;
        public const int InterpNum = 0x1E;
        public const int InterpVer = 0x1F;
        public const int ScreenHeight = 0x20;
        public const int ScreenWidth = 0x21;
        public const int ScreenWidthU = 0x22;
        public const int ScreenHeightU = 0x24;
        public const int FontWidthU = 0x26;
        public const int FontHeightU = 0x27;
        public const int RoutinesOffset = 0x28;
        public const int StaticStringsOffset = 0x2A;
        public const int BackgroundDef = 0x2C;
        public const int ForegroundDef = 0x2D;
        public const int TerminatingTable = 0x2E;
        public const int PixelWidth = 0x30;         // stream 3
        public const int RevisionNumber = 0x32;
        public const int AlphabetTable = 0x34;
        public const int HeaderExtTable = 0x36;
    }
}
