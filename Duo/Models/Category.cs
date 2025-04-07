using System;
using System.Collections.Generic;

namespace Duo.Models
{
    // test pr
    public class Category
    {
        private int _id;
        private string _name;

        public Category(int id, string name)
        {
            _id = id;
            _name = name;
        }
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}