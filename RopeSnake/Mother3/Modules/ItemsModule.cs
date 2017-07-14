using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
using RopeSnake.Mother3.Text;

namespace RopeSnake.Mother3
{
    public sealed class ItemsModule : ModuleBase
    {
        internal const int ItemCount = 256;

        public override string Name => "Mother3.Items";

        public override CompileResult Compile(ProjectData data, RomType romType)
        {
            var result = new CompileResult { RomType = romType };
            var items = (data as ItemsProjectData).Items;

            var itemsBlock = new Block(ItemCount * Item.SizeInBytes);
            var itemsStream = itemsBlock.ToStream();

            for (int i = 0; i < ItemCount; i++)
            {
                items[i].Index = i;
                itemsStream.WriteItem(items[i]);
            }

            var itemNames = new FixedStringTable
            {
                Strings = items.Select(i => i.Name).ToList(),
                StringLength = 9
            };

            var itemDescriptions = items.Select(i => i.Description).ToArray();

            throw new NotImplementedException();
        }

        public override IEnumerable<Range> GetFreeRanges(RomType romType)
            => Enumerable.Empty<Range>();

        public override bool IsCompatibleWith(RomType romType)
            => romType.Game == "Mother 3";

        public override ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource)
        {
            var data = new ItemsProjectData();

            using (var itemsFile = openResource("items", "json"))
                data.Items = itemsFile.ReadJson<Item[]>();

            return data;
        }

        public override ProjectData ReadFromRom(Rom rom)
        {
            var data = new ItemsProjectData();

            var itemNames = Mother3Helpers.ReadFixedStringTableFromTextTable(rom, 2);
            var itemDescriptions = Mother3Helpers.ReadStringTableFromTextTable(rom, 3);

            var itemTable = new FixedTableAccessor(Mother3Config.Configs[rom.Type].GetAsmPointer("ItemTable", rom),
                Item.SizeInBytes, ItemCount);

            data.Items = itemTable.ParseEntries(rom, s => s.ReadItem()).ToArray();

            foreach (var item in data.Items)
            {
                item.Name = itemNames.Strings[item.Index];
                item.Description = itemDescriptions[item.Index];
            }

            return data;
        }

        public override void WriteToProject(ProjectData data, RomType romType, OpenResourceDelegate openResource)
        {
            var dataTables = data as ItemsProjectData;

            using (var itemsFile = openResource("items", "json"))
                itemsFile.WriteJson(dataTables.Items);
        }

        public override void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ItemsProjectData : ProjectData
    {
        public Item[] Items { get; set; } = new Item[256];
    }
}
