using Celeste;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Security.Policy;

namespace BrokemiaHelper {
    public static class Extensions {
        //Thanks to Viv for stealing this from JaThePlayer who made the class allowing for StateMachine States to be added.
        private static FieldInfo StateMachine_begins = typeof(StateMachine).GetField("begins", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo StateMachine_updates = typeof(StateMachine).GetField("updates", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo StateMachine_ends = typeof(StateMachine).GetField("ends", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo StateMachine_coroutines = typeof(StateMachine).GetField("coroutines", BindingFlags.Instance | BindingFlags.NonPublic);

        public static int AddState(this StateMachine machine, Func<int> onUpdate, Func<IEnumerator> coroutine = null, Action begin = null, Action end = null) {
            Action[] begins = (Action[])StateMachine_begins.GetValue(machine);
            Func<int>[] updates = (Func<int>[])StateMachine_updates.GetValue(machine);
            Action[] ends = (Action[])StateMachine_ends.GetValue(machine);
            Func<IEnumerator>[] coroutines = (Func<IEnumerator>[])StateMachine_coroutines.GetValue(machine);
            int nextIndex = begins.Length;
            Array.Resize(ref begins, begins.Length + 1);
            Array.Resize(ref updates, begins.Length + 1);
            Array.Resize(ref ends, begins.Length + 1);
            Array.Resize(ref coroutines, coroutines.Length + 1);
            StateMachine_begins.SetValue(machine, begins);
            StateMachine_updates.SetValue(machine, updates);
            StateMachine_ends.SetValue(machine, ends);
            StateMachine_coroutines.SetValue(machine, coroutines);
            machine.SetCallbacks(nextIndex, onUpdate, coroutine, begin, end);
            return nextIndex;
        }

        public static int AddState(this StateMachine machine, Func<Player, int> onUpdate, Func<Player, IEnumerator> coroutine = null, Action<Player> begin = null, Action<Player> end = null) {
            Action[] begins = (Action[])StateMachine_begins.GetValue(machine);
            Func<int>[] updates = (Func<int>[])StateMachine_updates.GetValue(machine);
            Action[] ends = (Action[])StateMachine_ends.GetValue(machine);
            Func<IEnumerator>[] coroutines = (Func<IEnumerator>[])StateMachine_coroutines.GetValue(machine);
            int nextIndex = begins.Length;
            Array.Resize(ref begins, begins.Length + 1);
            Array.Resize(ref updates, begins.Length + 1);
            Array.Resize(ref ends, begins.Length + 1);
            Array.Resize(ref coroutines, coroutines.Length + 1);
            StateMachine_begins.SetValue(machine, begins);
            StateMachine_updates.SetValue(machine, updates);
            StateMachine_ends.SetValue(machine, ends);
            StateMachine_coroutines.SetValue(machine, coroutines);
            Func<IEnumerator> _coroutine = null;
            if (coroutine != null) {
                _coroutine = () => coroutine(machine.Entity as Player);
            }
            machine.SetCallbacks(nextIndex, () => onUpdate(machine.Entity as Player), _coroutine, () => begin(machine.Entity as Player), () => end(machine.Entity as Player));
            return nextIndex;
        }

        // https://github.com/CommunalHelper/CommunalHelper/blob/c6660d9c4a2c6c280c14b89e1806f3c3ae72d286/src/Utils/Extensions.cs#L379-L408
        public static void ForceAdd(this EntityList list, params Entity[] entities) {
            Scene scene = list.Scene;

            foreach (Entity entity in entities) {
                if (!list.current.Contains(entity)) {
                    list.current.Add(entity);
                    list.entities.Add(entity);
                    if (scene != null) {
                        scene.TagLists.EntityAdded(entity);
                        scene.Tracker.EntityAdded(entity);
                        entity.Added(scene);
                    }
                }
            }

            list.entities.Sort(EntityList.CompareDepth);

            foreach (Entity entity in entities) {
                if (entity.Scene == scene)
                    entity.Awake(scene);
            }
        }

        public static void DrawJustifiedClipped(this MTexture self, Vector2 position, Vector2 justify, Rectangle clip) {
            self.DrawJustifiedClipped(position, justify, clip, Vector2.One);
        }

        public static void DrawJustifiedClipped(this MTexture self, Vector2 position, Vector2 justify, Rectangle clip, Vector2 scale, float rotation = 0) {
            float scaleFix = self.ScaleFix;
            Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position,
                new Rectangle(self.ClipRect.X + clip.X, self.ClipRect.Y + clip.Y, clip.Width, clip.Height),
                Color.White, rotation, (new Vector2(self.Width * justify.X, self.Height * justify.Y) - self.DrawOffset) / scaleFix, scaleFix * scale, SpriteEffects.None, 0f);
        }

        public static int SimpleHash(this string str) {
            unchecked {
                int hash = 17;
                hash = hash * 31 + str.Length;
                foreach (char c in str) hash = hash * 31 + c;
                return hash;
            }
        }

        public static unsafe int WindowsHashCode(this string self) {
            unsafe {
                fixed (char* src = self) {
                    Contract.Assert(src[self.Length] == '\0', "src[this.Length] == '\\0'");
                    Contract.Assert(((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary");

                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;

                    // 32 bit machines.
                    int* pint = (int*)src;
                    int len = self.Length;
                    while (len > 2) {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }

                    if (len > 0) {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    }

                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
    }
}
