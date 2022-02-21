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
    /// Represents a barrier for cursor movement in one direction and one dimension.
    /// So you would need 4 to constrain to a screen.
    /// <para />
    /// TODO: is it worth making this abstract?
    /// </summary>
    internal class CursorBarrier
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CursorBarrier" /> class.
        /// </summary>
        /// <param name="active">Indicates if the barrier is active.  If false, the other parameters are ignored.</param>
        /// <param name="limit">This is the upper limit which we try to keep the cursor above. 
        /// Note this value is inclusive so the cursor is allowed to reach this value, but no higher.</param>
        /// <param name="minForce">This is the amount of force required to break through the barrier.
        /// If this is Int32.MaxValue then the barrier is solid and no amount of movement can break through it.
        /// Otherwise it represents the number of extra screen pixels the cursor has to move before
        /// we allow the cursor to break through the barrier.</param>
        public CursorBarrier(bool active, int limit, int minForce)
        {
            ChangeBarrier(active, limit, minForce);
        }

        /// <summary>
        /// Gets a value indicating whether the barrier is active
        /// </summary>
        protected bool Active { get; private set; }

        /// <summary>
        /// Gets the value for the edge of the barrier
        /// </summary>
        protected int Limit { get; private set; }

        /// <summary>
        /// Gets the minimum force needed to break through the barrier
        /// </summary>
        protected int MinForce { get; private set; }

        /// <summary>
        /// Gets or sets the total force so far expended trying to get through the barrier
        /// </summary>
        protected int TotalForce { get; set; }

        /// <summary>
        /// Changes the barrier values without the need to re-allocate a new barrier
        /// </summary>
        /// <param name="active">Indicates if the barrier is active.  If false, the other parameters are ignored.</param>
        /// <param name="limit">This is the lower limit which we try to keep the cursor above. 
        /// Note this value is inclusive so the cursor is allowed to reach this value, but no lower.</param>
        /// <param name="minForce">This is the amount of force required to break through the barrier.
        /// If this is Int32.MaxValue then the barrier is solid and no amount of movement can break through it.
        /// Otherwise it represents the number of extra screen pixels the cursor has to move before
        /// we allow the cursor to break through the barrier.</param>
        public void ChangeBarrier(bool active, int limit, int minForce)
        {
            // as no locking is performed 
            // if state goes from !active -> active - do this at the end
            // if state goes from active -> !active - do this at the start
            if (!active)
            {
                Active = active;
            }

            Limit = limit;
            MinForce = minForce;
            TotalForce = 0;
            if (active)
            {
                Active = active;
            }
        }
    }
}
