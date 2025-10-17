using EnglishLearning.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnglishLearning.Business.Model
{
    public class EnglishMediaForEditing: V_EnglishMedia
    {
        private bool isEditing;
        private bool isSelected;

        public bool IsEditing
        {
            get
            {
                return this.isEditing;
            }

            set
            {
                if (value != this.isEditing)
                {
                    this.isEditing = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        public bool TeacherNameIsVisible { get; set; } = true;
        public int TitleRowSpan { get; set; } = 1;
       
        public double Progress { get; set; }
    }
}
