using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using PlayWithYourPeas.Data;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// This displays the toolbox from which the player can select the blocks to place,
    /// or the delete option. Should be contained within the hud.
    /// </summary>
    internal class SpriteToolbox : Sprite
    {
        protected BlockType[] _toolTypes;
        protected Sprite[] _tools;
        protected Sprite _selection;
        protected Int32 _selectionIndex;
        protected Vector2 _displaySelectionPosition;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="layer"></param>
        public SpriteToolbox(SceneLayer layer) : base(layer, "Graphics/Tool-Background")
        {
            _toolTypes = new BlockType[] { BlockType.Normal, BlockType.Gel, BlockType.LeftRamp, BlockType.RightRamp, BlockType.Spring, BlockType.Delete };
            _tools = new Sprite[_toolTypes.Length];

            for (Int32 i = 0; i < _toolTypes.Length; i++)
                _tools[i] = new Sprite(layer, "Graphics/Tool-" + AssetName(_toolTypes[i]));

            _selection = new Sprite(layer, "Graphics/Tool-Selected");
        }

        /// <summary>
        /// Get asset name for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static String AssetName(BlockType type)
        {
            switch (type)
            {
                case BlockType.Normal:
                    return "StandardBlock";
                case BlockType.Gel:
                    return "GelBlock";
                case BlockType.Spring:
                    return "Spring";
                case BlockType.LeftRamp:
                    return "LeftRamp";
                case BlockType.RightRamp:
                    return "RightRamp";
                default:
                    return "Delete";
            }
        }

        /// <summary>
        /// Initialize the sprite
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            for (Int32 i = 0; i < _toolTypes.Length; i++)
                _tools[i].Initialize();
            _selection.Initialize();
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager manager)
        {
            base.LoadContent(manager);

            for (Int32 i = 0; i < _toolTypes.Length; i++)
                _tools[i].LoadContent(manager);
                
            _selection.LoadContent(manager);
        }

        /// <summary>
        /// Unloads all unmanaged content
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            for (Int32 i = 0; i < _toolTypes.Length; i++)
                _tools[i].UnloadContent();

            _selection.UnloadContent();
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _displaySelectionPosition = Vector2.Lerp(_displaySelectionPosition, Vector2.UnitY * (50 * _selectionIndex + 4), (Single)gameTime.ElapsedGameTime.TotalSeconds * 10);
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(GameTime gameTime, Vector2 offset)
        {
            base.Draw(gameTime, offset);

            for (Int32 i = 0; i < _toolTypes.Length; i++)
                _tools[i].Draw(gameTime, offset - this.Position - Vector2.UnitY * (50 * i + 6) - Vector2.UnitX * 7);

            _selection.Draw(gameTime, offset - this.Position - _displaySelectionPosition - Vector2.UnitX * 5);
        }

        /// <summary>
        /// Selects a block at a cursor position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public BlockType Select(Vector2 position)
        {
            _selectionIndex = Math.Min(_toolTypes.Length - 1, Math.Max(0, (Int32)(position.Y - 5) / 50));
            return _toolTypes[_selectionIndex];
        }
    }
}
