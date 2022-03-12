using System;
using Celeste;
using Monocle;

namespace BrokemiaHelper
{
    public enum CassetteModes
    {
        Solid,
        Leaving,
        Disabled,
        Returning
    }

    public interface CassetteEntity
    {

        EntityID ID
        {
            get;
            set;
        }

        int Index
        {
            get;
            set;
        }

        bool Activated
        {
            get;
            set;
        }

        float Tempo
        {
            get;
            set;
        }

        void MoveVExact(int move);

        void Finish();

        void SetActivatedSilently(bool activated);

        void WillToggle();
    }
}
