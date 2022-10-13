using Celeste;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
