using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
using RopeSnake.Mother3.Text;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class ItemsModule : ModuleBase
    {
        public override string Name => "Mother3.Items";

        public override CompileResult Compile(ProjectData data, RomType romType)
        {
            var result = new CompileResult(romType);
            var itemData = data as ItemsProjectData;
            var items = itemData.Items;

            if (items.Length > 256)
                RLog.Warn($"Too many items! Expected 256, got {items.Length}. Only writing the first 256...");

            int itemCount = Math.Min(256, items.Length);
            var itemsBlock = new Block(itemCount * Item.SizeInBytes);
            for (int i = 0; i < itemCount; i++)
            {
                items[i].Index = i;
                itemsBlock.WriteItem(i * Item.SizeInBytes, items[i]);
            }

            var itemNamesBlock = new Block(4 + (items.Length * itemData.NameLength * 2));
            var writer = Mother3TextWriter.Create(itemNamesBlock, romType);
            var itemNames = new FixedStringTable
            {
                Strings = items.Select(i => i.Name).ToList(),
                StringLength = itemData.NameLength
            };
            itemNamesBlock.WriteFixedStringTable(0, itemNames, writer);

            var itemDescriptionsBlock = new Block(0x10000);
            writer = Mother3TextWriter.Create(itemDescriptionsBlock, romType);
            var itemDescriptions = items.Select(i => i.Description).ToArray();
            int totalSize = itemDescriptionsBlock.WriteStringTable(0, writer, itemDescriptions).totalSize;
            itemDescriptionsBlock.Resize(totalSize);

            result.StaticBlocks.Add("ItemTable", (itemsBlock, itemData.ItemsOffset));
            result.AllocateBlocks.Add("ItemNames", itemNamesBlock);
            result.AllocateBlocks.Add("ItemDescriptions", itemDescriptionsBlock);

            return result;
        }

        public override IEnumerable<Range> GetFreeRanges(RomType romType)
            => Mother3Config.Configs[romType].FreeRanges["Items"];

        public override bool IsCompatibleWith(RomType romType)
            => romType.Game == "Mother 3";

        public override ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource)
        {
            ItemsProjectData itemData;

            using (var metaFile = openResource("items", "meta"))
                itemData = metaFile.ReadJson<ItemsProjectData>();

            using (var itemsFile = openResource("items", "json"))
                itemData.Items = itemsFile.ReadJson<Item[]>();

            return itemData;
        }

        public override ProjectData ReadFromRom(Rom rom)
        {
            var itemData = new ItemsProjectData();

            var itemNames = Mother3Helpers.ReadFixedStringTableFromTextTable(rom, 2);
            var itemDescriptions = Mother3Helpers.ReadStringTableFromTextTable(rom, 3);

            int itemsOffset = rom.GetAsmPointer("ItemTable");
            var itemTable = new FixedTable(itemsOffset, Item.SizeInBytes, 256);
            itemData.Items = itemTable.GetEntries().Select(e => rom.ReadItem(e.Offset)).ToArray();

            foreach (var item in itemData.Items)
            {
                item.Name = itemNames.Strings[item.Index];
                item.Description = itemDescriptions[item.Index];
            }

            itemData.NameLength = itemNames.StringLength;
            itemData.ItemsOffset = itemsOffset;

            return itemData;
        }

        public override void WriteToProject(ProjectData data, RomType romType, OpenResourceDelegate openResource)
        {
            var itemData = data as ItemsProjectData;

            using (var itemsFile = openResource("items", "json"))
                itemsFile.WriteJson(itemData.Items);

            using (var metaFile = openResource("items", "meta"))
                metaFile.WriteJson(itemData);
        }

        public override void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult)
        {
            CopyAllocatedBlocksToRom(rom, compileResult, allocationResult);
            CopyStaticBlocksToRom(rom, compileResult);
        }
    }

    public sealed class ItemsProjectData : ProjectData
    {
        [JsonIgnore]
        public Item[] Items { get; set; } = new Item[256];

        public int NameLength { get; set; }
        public int ItemsOffset { get; set; }
    }
}
