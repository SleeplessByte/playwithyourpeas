using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using System.Diagnostics;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Engine.Logic.AStar;

namespace PlayWithYourPeas.Logic
{
    /// <summary>
    /// AI for the peas. Some methods could be more efficient but they JustWork(tm) so
    /// why change them? Use register to be able to add a pea to collision checking for
    /// placement for blocks.
    /// </summary>
    internal class PeaController : GameComponent
    {
        protected TimeController _timeController;
        protected HashSet<DataPea> _peas;
        //protected Task _targetTask;
        protected DataGrid _grid;

        protected List<DataJumpSpot>[] _destinations;
        protected Dictionary<MoveNode, List<Node<MoveNode>>> _neighbours;
        protected Dictionary<DataPea, PeaControllerState> _snapshots;

        /// <summary>
        /// 
        /// </summary>
        public Boolean InnerEnabled { set { foreach (var pea in _peas) { pea.Enabled = value; } } }

        /// <summary>
        /// 
        /// </summary>
        public Single RunningSpeed { get { return (_timeController != null ? _timeController.Speed : 1); } }

        /// <summary>
        /// Constructs a new controller
        /// </summary>
        /// <param name="grid">DataGrid</param>
        public PeaController(Game game, DataGrid grid) : base(game)
        {
            _peas = new HashSet<DataPea>();
            _grid = grid;

            _neighbours = new Dictionary<MoveNode, List<Node<MoveNode>>>();
            _destinations = new List<DataJumpSpot>[DataGrid.Height];
            _snapshots = new Dictionary<DataPea, PeaControllerState>();

            for (int i = 0; i < _destinations.Length; i++)
                _destinations[i] = new List<DataJumpSpot>();

            FindTargets();

            _grid.OnGridChanged += new OnDataGridChangedHandler(_grid_OnGridChanged);
        }

        /// <summary>
        /// Executed when the grid changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _grid_OnGridChanged(object sender, EventArgs e)
        {
            _neighbours.Clear();

            FindTargets();

            // Remove targets no longer available
            foreach (var snapshot in _snapshots)
                if (snapshot.Value.Target != null)
                {
                    if (!_destinations[snapshot.Value.Target.Source.Y].Any(s => 
                        s.Source.X == snapshot.Value.Target.Source.X &&
                        s.Placement == snapshot.Value.Target.Placement))
                    {
                        snapshot.Value.Target = null;
                        snapshot.Value.EndPath();
                    }
                }
        }

        /// <summary>
        /// Registers a pea
        /// </summary>
        /// <param name="pea"></param>
        public void Register(DataPea pea)
        {
            _peas.Add(pea);
            _snapshots.Add(pea, new PeaControllerState());

            pea.OnRevive += new EventHandler(pea_OnRevive);
        }

        /// <summary>
        /// Deregisters all peas
        /// </summary>
        /// <returns></returns>
        public List<DataPea> DeRegisterAll()
        {
            var result = _peas.ToList();

            _peas.Clear();
            _snapshots.Clear();

            foreach (var pea in result)
                pea.OnRevive -= pea_OnRevive;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnRevive(object sender, EventArgs e)
        {
            // Blast hole where pea is appearing
            if (!_grid.StartRemovingAt((sender as DataPea).GridPosition))
            {
                (sender as DataPea).Revive();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _timeController = this.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// Updates a pea
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(DataPea pea, GameTime gameTime)
        {
            if (!this.Enabled)
                return;

            // If dead, move up
            if (!pea.IsAlive)
            {
                if (!pea.IsDying)
                {
                    pea.Position -= Vector2.UnitY * (Single)(gameTime.ElapsedGameTime.TotalSeconds * 100 * RunningSpeed);
                    if (pea.Position.Y + 200 < -60)
                        pea.Revive();
                }
                return;
            }

            // Update task limiter
            var snapshot = GetSnapshot(pea);
            snapshot.UpdateTargetLimit(gameTime, RunningSpeed);

            // Wait for rehab to pass
            if (pea.IsRehabilitating)
            {
                // If there is still a target
                if (snapshot.Target != null || !GetSnapshot(pea).WanderingNode.HasValue)
                {
                    // Update the wandering node to the arrived location
                    if (IsValid(pea.GridPosition))
                    {
                        var block = pea.GridPosition.Y >= 0 ? _grid.Grid[pea.GridPosition.X][pea.GridPosition.Y] : new DataBlock(new Point(pea.GridPosition.X, pea.GridPosition.Y - 1), BlockType.None, null);
                        var wanderto = CreateNode(block, MoveNode.Type.Walk, MoveNode.Direction.None).Value;
                        snapshot.Wander(wanderto, wanderto);
                    }
                    else
                    {
                        pea.Die();
                    }

                    snapshot.Target = null;
                }
                
                //pea.Rotation = Math.Min((Single)(Math.PI * 2), pea.Rotation + (Single)gameTime.ElapsedGameTime.TotalSeconds * 5);
                return;
            }

            // If we still can move
            if (snapshot.HasRemainingPath)
            {
                if (UpdateMoveTo(pea, gameTime) && snapshot.Target == null && !snapshot.HasRemainingPath)
                    FindWanderNode(pea, new Node<MoveNode>() { Value = snapshot.PreviousNode });
            }
            // else Wander
            else if (UpdateMoveTo(pea, gameTime))
            {
                // If no target and pathfinding is available.
                // What I do here is chain the pathfinding tasks so they don't take up so
                // much CPU. Pathfinding isn't that expensive but I didn't do a great job on it.

                // Reached wander node so lets find a new path
                if (snapshot.Target == null) // && (_targetTask == null || _targetTask.IsCompleted) && snapshot.TargetLimit <= 0)
                    //_targetTask = Task.Factory.StartNew(() => FindTarget(pea)).ContinueWith((a) => snapshot.ResetTargetLimit());

                    if (snapshot.TargetLimit <= 0)
                    {
                        if (!FindTarget(pea))
                            // Also find a new wander node so we can wander while it searches.
                            // when the findTarget function finishes, replaces the nodelist so 
                            // the next iteration the movement WILL go to the target
                            FindWanderNode(pea);

                        snapshot.ResetTargetLimit();
                    }
                    else
                    {
                        FindWanderNode(pea);
                    }
            }
            
        }

        /// <summary>
        /// Gets the controller state for a pea
        /// </summary>
        /// <param name="pea"></param>
        /// <returns></returns>
        protected PeaControllerState GetSnapshot(DataPea pea)
        {
            return _snapshots[pea];
        }
        
        /// <summary>
        /// Update moving towards a goal
        /// </summary>
        /// <param name="pea"></param>
        /// <param name="gameTime"></param>
        public Boolean UpdateMoveTo(DataPea pea, GameTime gameTime)
        {
            if (RunningSpeed <= 0)
                return false;

            var snapshot = GetSnapshot(pea);
            var previousNode = snapshot.PreviousNode;
            var nextNode = snapshot.HasRemainingPath ? snapshot.CurrentNode : snapshot.WanderingNode.Value;
            var reached = false;

            // If no longer valid move
            if (!IsValid(previousNode, nextNode))
            {
                // Detroy target, node list and so on
                snapshot.Target = null;
                snapshot.EndPath();
                return true;
            }

            // Branch move action
            switch (nextNode.Action)
            {
                case MoveNode.Type.Round:
                case MoveNode.Type.Climb:
                case MoveNode.Type.Wander:
                case MoveNode.Type.Ramp:
                    var dclimbmovement = (Single)(gameTime.ElapsedGameTime.TotalSeconds * (nextNode.Action == MoveNode.Type.Wander ? 70 : 50)) * RunningSpeed;
                    var dclimb = (nextNode.ScreenPosition - pea.Position);

                    if (dclimb != Vector2.Zero)
                    {

                        var climbdir = Vector2.Normalize(dclimb);
                        var dclimbdt = climbdir * dclimbmovement;

                        dclimbdt = dclimbdt.Length() < dclimb.Length() ? dclimbdt : dclimb;

                        pea.Rotation += (Single)(gameTime.ElapsedGameTime.TotalSeconds * 4) * RunningSpeed * 
                            (climbdir).Length() * (nextNode.Dir == MoveNode.Direction.Left ? -1 : 1);
                        pea.Position = pea.Position + dclimbdt;
                    }

                    reached = Math.Abs((dclimb).Length()) < 1f * RunningSpeed;
                   
                    break;

                /*case MoveNode.Type.Jump:
                    // TODO: path
                    var jlength = Math.Abs((nextNode.ScreenPosition - previousNode.ScreenPosition).X);
                    var djumpmovement = (Single)(gameTime.ElapsedGameTime.TotalSeconds * 2.5f);
                    snapshot.JumpAmount += djumpmovement / (jlength / 70);
                    var djumpwalk = (nextNode.ScreenPosition - pea.Position);
                    var jumpdir = Vector2.Normalize(djumpwalk);
                   
                    var jamount = Math.Max(0, Math.Min(1, (1 - Math.Abs(djumpwalk.X) / jlength)));

                    if (djumpwalk != Vector2.Zero)
                    {
                        pea.Rotation += (Single)(gameTime.ElapsedGameTime.TotalSeconds * 7) * (jumpdir).X;

                        
                        pea.Position = Vector2.Hermite(previousNode.ScreenPosition, -Vector2.UnitY * 50, nextNode.ScreenPosition, Vector2.UnitY * 50 , snapshot.JumpAmount);

                        //if (jamount < 0.4f) { 
                            //pea.Position = Vector2.Lerp(pea.Position, nextNode.ScreenPosition - Vector2.UnitY * 60, djumpmovement / (jlength / 70f)); //pea.Position + jumpdir * djumpmovement;
                            Vector2.CatmullRom(previousNode.ScreenPosition + Vector2.UnitY * 5,
                                previousNode.ScreenPosition,
                                nextNode.ScreenPosition,
                                nextNode.ScreenPosition + Vector2.UnitY * 5,
                                jamount);
                        //} else {
                            //pea.Position = Vector2.Lerp(Vector2.Lerp(nextNode.ScreenPosition, nextNode.ScreenPosition - Vector2.UnitY * 60, 0.8f),
                            //    nextNode.ScreenPosition, jamount + djumpmovement * 0.005f);
                        //}
                    
                        reached = Math.Abs((djumpwalk).Length()) < 0.8f;
                        
                    } else {
                        reached = true;
                    }


                    if (reached)
                        snapshot.JumpAmount = 0;
                   

                    break;*/

                case MoveNode.Type.None:
                case MoveNode.Type.Walk:
                default:
                    var dwalkmovement = (Single)(gameTime.ElapsedGameTime.TotalSeconds * 70) * RunningSpeed;
                    var dwalk = (nextNode.ScreenPosition - pea.Position);
                    var walkdir = Vector2.Normalize(dwalk);

                    if (dwalk != Vector2.Zero)
                    {
                        var dwalkdt = walkdir * dwalkmovement;
                        dwalkdt = dwalkdt.Length() < dwalk.Length() ? dwalkdt : dwalk;

                        pea.Rotation += (Single)(gameTime.ElapsedGameTime.TotalSeconds * 4) * RunningSpeed * (walkdir).X;;
                        pea.Position = pea.Position + dwalkdt;
                    }

                    reached = Math.Abs((dwalk).Length()) < Math.Min(30f, 1f * RunningSpeed);
                    break;
            }

            // If reached node, dequeue
            if (reached)
                snapshot.ProgressPath(); 

            // Jump if reached spot
            if (!snapshot.HasRemainingPath && snapshot.Target != null)
                Jump(pea, gameTime);

            return reached;
        }

        /// <summary>
        /// Jumping start
        /// </summary>
        /// <param name="pea"></param>
        /// <param name="gameTime"></param>
        public void Jump(DataPea pea, GameTime gameTime)
        {
            var snapshot = GetSnapshot(pea);
            var d = (snapshot.Target.Source.Position + Vector2.UnitX * 35 - Vector2.UnitY * 8 - pea.Position);

            // If not in range of the target, stop jumping
            if (d.Length() > 5 || !IsValid(snapshot.Target) || 
                ((snapshot.Target.Placement == DataJumpSpot.Location.Left ? 
                    (snapshot.Target.Source == null ? snapshot.Target : snapshot.Target.Source.JumpLeft)  :
                    (snapshot.Target.Source == null ? snapshot.Target : snapshot.Target.Source.JumpRight)) ?? snapshot.Target).HasJumped(pea))
            {
                // TODO REMOVE JUMPSPOT FROM CANDIDATES
                snapshot.Target = null;
                return;
            }

            pea.Jump(snapshot.Target);
        }

        /// <summary>
        /// Finds all jump spots
        /// </summary>
        private void FindTargets()
        {
            var grid = _grid.Grid;
            for (Int32 y = 0; y < grid[0].Length; y++)
            {
                _destinations[y].Clear();
                for (Int32 x = 0; x < grid.Length; x++)
                {
                    DataBlock now = grid[x][y];
                    if (now != null)
                    {
                        // Find spots
                        if (now.JumpLeft != null)
                        {
                            if (now.JumpLeft.Completion < 1)
                                _destinations[y].Add(now.JumpLeft);
                        }
                        else if (IsJumpSpot(now, DataJumpSpot.Location.Left))
                        {
                            _destinations[y].Add(new DataJumpSpot(grid[x][y]) { Placement = DataJumpSpot.Location.Left });
                        }

                        if (now.JumpRight != null)
                        {
                            if (now.JumpRight.Completion < 1)
                                _destinations[y].Add(now.JumpRight);
                        }
                        else if (IsJumpSpot(now, DataJumpSpot.Location.Right))
                        {
                            _destinations[y].Add(new DataJumpSpot(grid[x][y]) { Placement = DataJumpSpot.Location.Right });
                        }
                    }
                }

                //Debug.WriteLine("Found {0} spots for y = {1}", _destinations[y].Count, y);
            }
        }

        /// <summary>
        /// Finds a jump target for a pea
        /// <param name="pea"></param>
        /// </summary>
        private Boolean FindTarget(DataPea pea)
        {
            IEnumerable<Tuple<Boolean, DataJumpSpot, Queue<MoveNode>>> spots = null; //new List<DataJumpSpot>();

            for (int y = 0; y < _destinations.Length; y++)
            {
                if (_destinations[y].Count > 0)
                {
                    // See if spots are available to pea and are reachable
                    var potential = _destinations[y].Where(spot => !spot.HasJumped(pea));
                    spots = potential.Select(spot => { 
                        var path = new Queue<MoveNode>(); 
                        Boolean r = IsReachable(pea, spot, out path); 
                        return new Tuple<Boolean, DataJumpSpot, Queue<MoveNode>>(r, spot, path); 
                    }).Where(a => a.Item1);

                    //Debug.WriteLine("Found {0} spots for y = {1} reachable", spots.Count(), y);

                    if (spots.Count() > 0)
                        break;
                }
            }

            var snapshot = GetSnapshot(pea);
        
            // Select a target
            if (spots != null)
            {
                var targets = spots.Where(spot => spot.Item2.Completion == spots.Max(cspot => cspot.Item2.Completion));
                var target = targets.Count() > 0 ? targets.ElementAt(DataPea.Random.Next(targets.Count())) : null;
                if (target != null)
                {
                    snapshot.Target = target.Item2;

                    // Get the path
                    snapshot.StartPath(target.Item3);
                }

                return target != null;
            }

            return false;
        }

        /// <summary>
        /// Finds a path to a jumping spot
        /// </summary>
        /// <param name="spot">Spot to jump</param>
        /// <param name="path">Path</param>
        /// <returns>Has found</returns>
        private Boolean FindPath(DataPea pea, DataJumpSpot spot, out Queue<MoveNode> path)
        {
            path = new Queue<MoveNode>();

            var startNode = new Node<MoveNode>() { Value = GetSnapshot(pea).PreviousNode }; //.Action == MoveNode.Type.None ? new Node<MoveNode>() { Value = new MoveNode() { Position = pea.GridPosition, Action = MoveNode.Type.Walk} } : ;
            var goalNode = new Node<MoveNode>() { Value = new MoveNode() { Position = new Point(spot.Source.X, spot.Source.Y - 1), Action = MoveNode.Type.Walk } };
            var domain = new List<Node<MoveNode>>();
            domain.Add(startNode);

            // Notes: Normally you would build the domain from ALL the walkable places and 
            // have the neighbourghing function run just once for each node. Because that
            // function is complex, I added it dynamically (runs on reach node). This changes
            // some of the behavior of A*. Don't forget that...

            // Debuging
            Solver<MoveNode>.SolverProgress += new EventHandler<SolverResultEventArgs<MoveNode>>(PeaController_SolverProgress);

            // Get a path
            var result = Solver<MoveNode>.Solve(
                startNode,
                goalNode,
                domain,
                CreateMoveNodesFrom, // This was initialially not in the solver
                n => Math.Abs(n.Value.Position.X - goalNode.Value.Position.X) + Math.Abs(n.Value.Position.Y - goalNode.Value.Position.Y),
                n => Math.Abs(n.Value.Position.X - startNode.Value.Position.X) + Math.Abs(n.Value.Position.Y - startNode.Value.Position.X),
                (n, m) => n.Value.Equals(m.Value),
#if DEBUG
                true
#else
                false
#endif
                );

            if (result == null)
            {
                path = null;
                return false;
            }

            foreach (Node<MoveNode> node in result)
                path.Enqueue(node.Value);

            return true;
        }

        /// <summary>
        /// Progress path solver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeaController_SolverProgress(object sender, SolverResultEventArgs<MoveNode> e)
        {
            //if ((Boolean)sender)
                DebugNodes = e.Result;
        }

        /// <summary>
        /// Debugging nodes
        /// </summary>
        public static List<Node<MoveNode>> DebugNodes;

        /// <summary>
        /// Neighbouring function
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected List<Node<MoveNode>> CreateMoveNodesFrom(Node<MoveNode> node) {
            var position = node.Value.Position;

            if (!IsValid(position))
                return new List<Node<MoveNode>>();
            
            var results = new List<Node<MoveNode>>();

            List<Node<MoveNode>> saved = null;
            if (_neighbours.TryGetValue(node.Value, out saved))
            {
                // Create nodes from cache
                foreach (var savedNode in saved)
                    results.Add(new Node<MoveNode> { Value = savedNode.Value });

                return results;
            }

            var movement = node.Value.Action;
            var direction = node.Value.Dir;
            var opositedir = direction == MoveNode.Direction.Left ? MoveNode.Direction.Right : MoveNode.Direction.Left;

            var current = position.Y >= 0 ? _grid.Grid[position.X][position.Y] : new DataBlock(new Point(position.X, position.Y), BlockType.None, null);
            var up = (position.Y > 0) ? _grid.Grid[position.X][position.Y - 1] : null;
            var upleft = (position.X > 0 &&position.Y > 0) ? _grid.Grid[position.X - 1][position.Y - 1] : null;
            var upright = (position.X < DataGrid.Width - 1 && position.Y > 0) ? _grid.Grid[position.X + 1][position.Y - 1] : null;

            var downleft = (position.X > 0 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X - 1][position.Y + 1] : null;
            var downleftleft = (position.X > 1 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X - 2][position.Y + 1] : null;
            var downright = (position.X < DataGrid.Width - 1 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X + 1][position.Y + 1] : null;
            var downrightright = (position.X < DataGrid.Width - 2 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X + 2][position.Y + 1] : null;

            // Here I create some "air" blocks if the position is above the grid. This could be neater, but it JustWorks(tm)
            var left = (position.X > 0) ? (position.Y >= 0 ? _grid.Grid[position.X - 1][position.Y] : new DataBlock(new Point(position.X - 1, position.Y), BlockType.None, null)) : null;
            var leftleft = (position.X > 1) ? (position.Y >= 0 ? _grid.Grid[position.X - 2][position.Y] : new DataBlock(new Point(position.X - 2, position.Y), BlockType.None, null)) : null;
            var right = (position.X < DataGrid.Width - 1) ? (position.Y >= 0 ?_grid.Grid[position.X + 1][position.Y] : new DataBlock(new Point(position.X + 1, position.Y), BlockType.None, null)) : null;
            var rightright = (position.X < DataGrid.Width - 2) ? (position.Y >= 0 ? _grid.Grid[position.X + 2][position.Y] : new DataBlock(new Point(position.X + 2, position.Y), BlockType.None, null)) : null;

            switch (movement)
            {
                case MoveNode.Type.None:
                case MoveNode.Type.Walk:
                case MoveNode.Type.Jump:

                    if (direction == MoveNode.Direction.None)
                    {
                        if (movement == MoveNode.Type.Jump)
                            results.Add(CreateNode(current, MoveNode.Type.Walk, MoveNode.Direction.None));

                        // Move and Jump
                        if (right != null && right.IsClear)
                            if (downright == null || downright.IsFlatTop)
                                results.Add(CreateNode(right, MoveNode.Type.Walk, MoveNode.Direction.None));
                            else if (downright.IsClear && downrightright != null && downrightright.IsFlatTop && (rightright == null || rightright.IsClear))
                                results.Add(CreateNode(rightright, MoveNode.Type.Jump, MoveNode.Direction.None));

                        // Go onto ramp
                        if (right != null && right.IsRampLeft)
                            if (upright == null || upright.IsClear)
                                results.Add(CreateNode(right, MoveNode.Type.Ramp, MoveNode.Direction.Right));

                        // Move and jump
                        if (left != null && left.IsClear)
                            if (downleft == null || downleft.IsFlatTop)
                                results.Add(CreateNode(left, MoveNode.Type.Walk, MoveNode.Direction.None));
                            else if (downleft.IsClear && downleftleft != null && downleftleft.IsFlatTop && (leftleft == null || leftleft.IsClear))
                                results.Add(CreateNode(leftleft, MoveNode.Type.Jump, MoveNode.Direction.None));

                        // Go onto ramp
                        if (left != null && left.IsRampRight)
                            if (upleft == null || upleft.IsClear)
                                results.Add(CreateNode(left, MoveNode.Type.Ramp, MoveNode.Direction.Left));

                        // Climb
                        if (right != null && right.IsSolidLeft)
                            results.Add(CreateNode(current, MoveNode.Type.Climb, MoveNode.Direction.Right));
                        if (left != null && left.IsSolidRight)
                            results.Add(CreateNode(current, MoveNode.Type.Climb, MoveNode.Direction.Left));

                        // Jump up
                        if (up != null && up.IsClear)
                            if (right != null && !right.IsSolidLeft && upright != null && upright.IsSolidLeft)
                                results.Add(CreateNode(up, MoveNode.Type.Jump, MoveNode.Direction.Right));
                            else if (left != null && !left.IsSolidRight && upleft != null && upleft.IsSolidRight)
                                results.Add(CreateNode(up, MoveNode.Type.Jump, MoveNode.Direction.Left));
                    }
                    else
                    {
                        var cjupdir = (direction == MoveNode.Direction.Left ? upleft : upright);
                        var cjdirflat = (direction == MoveNode.Direction.Left ? left != null && left.IsFlatTop : right != null && right.IsFlatTop);
                        var cjupval = cjupdir != null && (direction == MoveNode.Direction.Left ? cjupdir.IsSolidRight : cjupdir.IsSolidLeft);
                        if (up == null || up.IsClear)
                        {
                            // Climb higher
                            if (cjupval)
                                results.Add(CreateNode(up, MoveNode.Type.Climb, direction));
                            // Round top
                            else if (cjdirflat && (cjupdir == null || cjupdir.IsClear))
                                results.Add(CreateNode(up ?? new DataBlock(new Point(current.X, current.Y - 1), BlockType.None, null), MoveNode.Type.Round, direction));
                        }
                        break;
                    }
                        
                    break;

                case MoveNode.Type.Ramp:
                    var rrupdir = (direction == MoveNode.Direction.Left ? upleft : upright);
                    var rrdir = (direction == MoveNode.Direction.Left ? left : right);
                    var rrupdirval = (rrupdir != null && (direction == MoveNode.Direction.Left ? rrupdir.IsSolidRight : rrupdir.IsSolidLeft));
                    if (up != null && up.IsClear)
                    {
                        // Climb wall
                        if (rrupdir != null && rrupdirval)
                            results.Add(CreateNode(up, MoveNode.Type.Climb, direction));
                        // End of the ramp
                        else if (rrupdir != null && rrupdir.IsClear && rrdir != null && rrdir.IsFlatTop)
                            results.Add(CreateNode(up, MoveNode.Type.Round, direction));
                        // Another ramp 
                        else if (rrupdir != null && (direction == MoveNode.Direction.Left ? rrupdir.IsRampRight : rrupdir.IsRampLeft))
                            results.Add(CreateNode(rrupdir, MoveNode.Type.Ramp, direction));
                    }
                    break;

                case MoveNode.Type.Climb:
                    ///....U___
                    // ....^| right

                    var cupdir = (direction == MoveNode.Direction.Left ? upleft : upright);
                    var cdirflat = (direction == MoveNode.Direction.Left ? left != null && left.IsFlatTop : right != null && right.IsFlatTop);
                    var cupval = cupdir != null && (direction == MoveNode.Direction.Left ? cupdir.IsSolidRight : cupdir.IsSolidLeft);
                    if (up == null || up.IsClear)
                    {
                        // Climb higher
                        if (cupval)
                            results.Add(CreateNode(up, MoveNode.Type.Climb, direction));
                        // Round top
                        else if (cdirflat && (cupdir == null || cupdir.IsClear))
                            results.Add(CreateNode(up ?? new DataBlock(new Point(current.X, current.Y - 1), BlockType.None, null) , MoveNode.Type.Round, direction));
                    }
                    break;

                case MoveNode.Type.Round:
                    var rdir = (direction == MoveNode.Direction.Left ? left : right);
                    var rdirdown = (direction == MoveNode.Direction.Left ? downleft : downright);
                    // Round top
                    if (rdir != null && rdirdown != null && rdir.IsClear && !rdirdown.IsClear)
                        results.Add(CreateNode(rdir, MoveNode.Type.Walk, MoveNode.Direction.None));
                    // Round top and climb
                    else if (rdir != null && (direction == MoveNode.Direction.Left ? rdir.IsRampRight : rdir.IsRampLeft))
                        results.Add(CreateNode(rdir, MoveNode.Type.Ramp, direction));
                    else if (rdir != null && (direction == MoveNode.Direction.Left ? rdir.IsSolidRight : rdir.IsSolidLeft))
                        results.Add(CreateNode(current, MoveNode.Type.Climb, direction));
                    break;

                case MoveNode.Type.Wander:
                    if (direction == MoveNode.Direction.Left)
                        results.Add(CreateNode(current, MoveNode.Type.Wander, MoveNode.Direction.Right));
                    else
                        results.Add(CreateNode(current, MoveNode.Type.Walk, MoveNode.Direction.None));
                    break;
            }

            Debug.Assert(!_neighbours.Remove(node.Value));
            _neighbours.Add(node.Value, results);

            return results; 
        }

        
        /// <summary>
        /// Finds a node to wander
        /// </summary>
        /// <param name="pea"></param>
        /// <returns></returns>
        protected void FindWanderNode(DataPea pea, Node<MoveNode> prev = null)
        {
            var snapshot = GetSnapshot(pea);
            var block = pea.GridPosition.Y >= 0 ? _grid.Grid[pea.GridPosition.X][pea.GridPosition.Y] : new DataBlock(new Point(pea.GridPosition.X, pea.GridPosition.Y - 1), BlockType.None, null);
            var node = (prev == null) ? (snapshot.WanderingNode.HasValue && 
                snapshot.WanderingNode.Value.Action != MoveNode.Type.None && 
                snapshot.WanderingNode.Value.Position == pea.GridPosition 
                    ? new Node<MoveNode>() { Value = snapshot.WanderingNode.Value } 
                    : CreateNode(block, MoveNode.Type.Walk, MoveNode.Direction.None)
                ) : prev;
            var possibleNodes = CreateMoveNodesFrom(node);

            if (possibleNodes.Count > 0)
            {
                // Find nodes that go higher (non walk nodes and non lineair jumps)
                var nonwalk = possibleNodes.Where(a => a.Value.Action != MoveNode.Type.Walk && 
                    !((a.Value.Action == MoveNode.Type.Jump) && (a.Value.Dir == MoveNode.Direction.None)));

                // Limit to these if needed
                if (nonwalk.Count() > 0) {
                    possibleNodes = nonwalk.ToList();
                }
                else if (DataPea.Random.Next(100) < 10)
                {
                    snapshot.Wander(node.Value, null);
                    pea.Jump(null);
                    return;
                }

                snapshot.Wander(node.Value, possibleNodes[DataPea.Random.Next(possibleNodes.Count)].Value);
                
            }
            else
            {
                snapshot.Wander(node.Value, null);
                pea.Jump(null);
            }
        }

        /// <summary>
        /// Create a node for a move node
        /// </summary>
        /// <param name="position"></param>
        /// <param name="action"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        protected Node<MoveNode> CreateNode(DataBlock block, MoveNode.Type action, MoveNode.Direction dir)
        {
            return CreateNode(block.GridPosition, action, dir);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="action"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        protected Node<MoveNode> CreateNode(Point position, MoveNode.Type action, MoveNode.Direction dir)
        {
            var node = new MoveNode() { Position =position };
            node.Action = action;
            node.Dir = dir;
            return new Node<MoveNode>() { Value = node };
        }

        /// <summary>
        /// Finds a jump spot
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private Boolean IsJumpSpot(DataBlock spot, DataJumpSpot.Location location)
        {
            /// -----
            /// --X--
            /// -LSR-
            /// S = spot, X = jump location (up)
            ///
            /// upup should be clear
            /// up should be clear
            /// 
            /// left should be clear
            /// leftleft should be clear
            /// leftup should be clear
            /// leftleftup should be clear
            /// leftupup questionable

            var position = spot.GridPosition;
            var down = (position.Y < DataGrid.Height - 1) ? _grid.Grid[position.X][position.Y + 1] : null;
            var solid = (spot == null || spot.IsFlatTop);

            var upup = (position.Y > 1) ? _grid.Grid[position.X][position.Y - 2] : null;
            var up = (position.Y > 0) ? _grid.Grid[position.X][position.Y - 1] : null;
            
            var left = (position.X > 0) ? _grid.Grid[position.X - 1][position.Y] : null;
            var leftleft = (position.X > 1) ? _grid.Grid[position.X - 2][position.Y] : null;
            var leftup = (position.X > 0 && (position.Y > 0)) ? _grid.Grid[position.X - 1][position.Y - 1] : null;
            var leftdown = (position.X > 0 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X - 1][position.Y + 1] : null;
            var leftleftdown = (position.X > 1 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X - 2][position.Y + 1] : null;
            var leftleftup = (position.X > 1 && (position.Y > 0)) ? _grid.Grid[position.X - 2][position.Y - 1] : null;
            var leftupup = (position.X > 0 && (position.Y > 1)) ? _grid.Grid[position.X - 1][position.Y - 2] : null;
            
            var right = (position.X < DataGrid.Width - 1) ? _grid.Grid[position.X + 1][position.Y] : null;
            var rightright = (position.X < DataGrid.Width - 2) ? _grid.Grid[position.X + 2][position.Y] : null;
            var rightup = (position.X < DataGrid.Width - 1 && (position.Y > 0)) ? _grid.Grid[position.X + 1][position.Y - 1] : null;
            var rightdown = (position.X < DataGrid.Width - 1 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X + 1][position.Y + 1] : null;
            var rightrightdown = (position.X < DataGrid.Width - 2 && (position.Y < DataGrid.Height - 1)) ? _grid.Grid[position.X + 2][position.Y + 1] : null;
            var rightrightup = (position.X < DataGrid.Width - 2 && (position.Y > 0)) ? _grid.Grid[position.X + 2][position.Y - 1] : null;
            var rightupup = (position.X < DataGrid.Width - 1 && (position.Y > 1)) ? _grid.Grid[position.X + 1][position.Y - 2] : null;

            // Above and above that should be clear
            if (!solid || (up != null && (!up.IsClear || up.IsTransitioning)) || (upup != null && (!upup.IsClear || upup.IsTransitioning)))
                return false;

            if (location == DataJumpSpot.Location.Left)
                return ((left != null && left.IsClear && !left.IsTransitioning) &&
                    (leftleft != null && leftleft.IsClear && !leftleft.IsTransitioning) &&
                    (leftupup == null || (leftupup.IsClear && !leftupup.IsTransitioning)) &&
                    (leftleftup == null || (leftleftup.IsClear && !leftleftup.IsTransitioning)) &&
                    (leftup == null || (leftup.IsClear)));

            if (location == DataJumpSpot.Location.Right)
                return ((right != null && right.IsClear && !right.IsTransitioning) &&
                    (rightright != null && rightright.IsClear && !rightright.IsTransitioning) &&
                    (rightupup == null || (rightupup.IsClear && !rightupup.IsTransitioning)) &&
                    (rightrightup == null || (rightrightup.IsClear && !rightrightup.IsTransitioning)) &&
                    (rightup == null || (rightup.IsClear && !rightup.IsTransitioning)));

            return false;
        }

        /// <summary>
        /// Is reachable from spot
        /// </summary>
        /// <param name="spot"></param>
        /// <returns></returns>
        private Boolean IsReachable(DataPea pea, DataJumpSpot spot, out Queue<MoveNode> path)
        {
            // Question: Is it worth it to save paths found (need re-evaluation when grid
            // changes). Should test that later and work from there.

            if (!FindPath(pea, spot, out path))
                return false;

            return true;   
        }

        /// <summary>
        /// Is valid path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Boolean IsValid(DataPea pea, Queue<MoveNode> path)
        {
            // TODO: retry a path from a new position position
            if (path.Peek().Position != pea.GridPosition)
                return false;

            var valid = new Queue<MoveNode>();
            
            // Dummy previous node
            valid.Enqueue(CreateNode(pea.GridPosition, MoveNode.Type.Walk, MoveNode.Direction.None).Value); 

            while (path.Count > 0) {
                var node = path.Peek();
                // Check validity of a node itself
                if (!IsValid(node.Position))
                    return false;
                // Check validity of moving from to the node
                if (!IsValid(valid.LastOrDefault(), node))
                    return false;

                valid.Enqueue(path.Dequeue());
            }

            // Remove dummy
            valid.Dequeue();

            // Restore path
            while (valid.Count > 0)
                path.Enqueue(valid.Dequeue());

            return true;
        }
        
        /// <summary>
        /// Checks if node is still movable
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected Boolean IsValid(MoveNode from, MoveNode to)
        {
            // There was no previous node, so it's valid
            if (from.Action == MoveNode.Type.None)
                return true;

            // Already on the next node, so it's valid
            if (from.Equals(to))
                return true;

            // Look up if to is still a neighbour of from (pre-calc)
            List<Node<MoveNode>> neighbours = null;
            if (_neighbours.TryGetValue(from, out neighbours))
                return neighbours.Any(n => n.Value.Equals(to));

            // Fetch and look up
            return CreateMoveNodesFrom(new Node<MoveNode>() { Value = from }).Any(n => n.Value.Equals(to));
        }

        /// <summary>
        /// Is valid location (grid)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Boolean IsValid(Point point)
        {
            return (point.X >= 0 && point.Y >= -1 && point.X <= DataGrid.Width - 1 && point.Y <= DataGrid.Height  - 1);
        }

        /// <summary>
        /// Is still a valid jumpspot
        /// </summary>
        /// <param name="dataJumpSpot"></param>
        /// <returns></returns>
        protected Boolean IsValid(DataJumpSpot dataJumpSpot)
        {
            return (dataJumpSpot.Placement == DataJumpSpot.Location.Left ? dataJumpSpot == dataJumpSpot.Source.JumpLeft : dataJumpSpot == dataJumpSpot.Source.JumpRight) 
                || IsJumpSpot(dataJumpSpot.Source, dataJumpSpot.Placement);
        }

        /// <summary>
        /// Is any pea touching fixture
        /// </summary>
        /// <param name="fix"></param>
        /// <returns></returns>
        public Boolean IsTouching(Fixture fix)
        {
            return _peas.Any(p => p.IsTouching(fix));
        }
    }
}
