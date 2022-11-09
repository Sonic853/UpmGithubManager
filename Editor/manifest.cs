using System.Collections.Generic;

namespace Sonic853.UpmGithubManager
{
    class manifest
    {
        public Dictionary<string, string> dependencies;
        public bool enableLockFile;
        public string registry;
        public string resolutionStrategy;
        public scopedRegistries[] scopedRegistries;
        public string[] testables;
        public bool useSatSolver;
    }
}