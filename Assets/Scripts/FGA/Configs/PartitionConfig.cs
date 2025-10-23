// ==================================================
// Partition Config
// -----------
// A configuration of a Partition.
// ==================================================

namespace FGA.Configs
{
    public static class PartitionConfig
    {
        public static int MIN_PARTITION_SIZE = 20;
        public static int BASE_PARTITION_SIZE = 60;
        public static int PARTITION_MARGIN = 2;
        public static int PARTITION_GROWTH = 20;
        public static int MIN_PARTITION_SPLIT_SIZE = 40;
    }

    public enum SplitOrientation
    {
        None,
        Horizontal,
        Vertical
    }

    public enum NodeLevel
    {
        Root,
        Internal,
        Leaf
    }

    public enum NodeExposure
    {
        Root,
        Core,
        Edge,
        Corner
    }
}