#if UNSAFE

using System;

namespace Nucleus.Gaming
{

    public class FileNameAndParentFrn
    {
        #region Properties
        private string _name;
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        private ulong _parentFrn;
        public ulong ParentFrn
        {
            get => _parentFrn;
            set => _parentFrn = value;
        }
        #endregion

        #region Constructor
        public FileNameAndParentFrn(string name, ulong parentFrn)
        {
            if (name != null && name.Length > 0)
            {
                _name = name;
            }
            else
            {
                throw new ArgumentException("Invalid argument: null or Length = zero", "name");
            }
            if (!(parentFrn < 0))
            {
                _parentFrn = parentFrn;
            }
            else
            {
                throw new ArgumentException("Invalid argument: less than zero", "parentFrn");
            }
        }
        #endregion

    }
}
#endif