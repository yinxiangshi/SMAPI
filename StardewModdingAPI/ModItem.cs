using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewModdingAPI
{
    class ModItem : StardewValley.Object
    {
        public Item AsItem { get { return (Item) this; } }
        public override string Name { get; set; }
        public string Description { get; set; }
        public int ID { get; set; }
        public Texture2D Texture { get; set; }

        public ModItem()
        {
            
        }
    }
}
