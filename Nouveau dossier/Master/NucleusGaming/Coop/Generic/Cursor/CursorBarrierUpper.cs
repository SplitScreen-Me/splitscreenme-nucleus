#region copyright
// This file is part of Dual Monitor Tools which is a set of tools to assist
// users with multiple monitor setups.
// Copyright (C) 2010-2015  Gerald Evans
// 
// Dual Monitor Tools is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace Nucleus.Gaming.Coop.Generic.Cursor
{
    /// <summary>
    /// Represents an upper barrier for cursor movement.
    /// This is for 1D only, so 2 of these classes will be needed
    /// to constrain cursor movement.
    /// </summary>
    internal class CursorBarrierUpper : CursorBarrier
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CursorBarrierUpper" /> class.
        /// </summary>
        /// <param name="active">Indicates if the barrier is active.  If false, the other parameters are ignored.</param>
        /// <param name="limit">This is the upper limit which we try to keep the cursor above. 
        /// Note this value is inclusive so the cursor is allowed to reach this value, but no higher.</param>
        /// <param name="minForce">This is the amount of force required to break through the barrier.
        /// If this is Int32.MaxValue then the barrier is solid and no amount of movement can break through it.
        /// Otherwise it represents the number of extra screen pixels the cursor has to move before
        /// we allow the cursor to break through the barrier.</param>
        public CursorBarrierUpper(bool active, int limit, int minForce)
            : base(active, limit, minForce)
        {
        }

        /// <summary>
        /// Checks if the cursor has broken through the barrier.
        /// </summary>
        /// <param name="newValue">On entry this is the position that Windows wants to put the cursor.
        /// On exit we adjust this value if needed to limit the position by the barrier, or
        /// if it has broken through the barrier, we restrict the value to take into account
        /// the effort required to break through the barrier.</param>
        /// <returns>true if the cursor has broken through the barrier.</returns>
        public bool BrokenThrough(ref int newValue)
        {
            bool brokenThrough = false;
            if (Active)
            {
                if (newValue > Limit)
                {
                    if (MinForce == int.MaxValue)
                    {
                        // not allowed to break through barrier
                        newValue = Limit;
                    }
                    else
                    {
                        TotalForce += newValue - Limit;
                        if (TotalForce > MinForce)
                        {
                            // cursor has broken through barrier
                            newValue = Limit + TotalForce - MinForce;
                            brokenThrough = true;
                        }
                        else
                        {
                            newValue = Limit;
                        }
                    }
                }
                else
                {
                    TotalForce = 0;
                }
            }

            return brokenThrough;
        }

        /// <summary>
        /// Checks if the value is outside of the barrier.
        /// This does not take into account any effort required to break through the barrier
        /// </summary>
        /// <param name="newValue">new cursor position</param>
        /// <returns>true if the specified position is outside of the barrier</returns>
        public bool Outside(int newValue)
        {
            bool outside = false;
            if (Active)
            {
                if (newValue > Limit)
                {
                    outside = true;
                }
            }

            return outside;
        }
    }
}
