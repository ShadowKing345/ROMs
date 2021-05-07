using System;
using System.Text.Json;

namespace RestaurantOrderManagementSystem.Models.Table
{
    public class VisualBase
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Style { get; set; }
        public string Shape { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public VisualizerTypes Type { get; set; }
        public VisualBase[] Children { get; set; }
    }
}