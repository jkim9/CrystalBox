using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCACalibrator.Setting
{
    public class MCACalibratorSettings : ApplicationSettingsBase
    {
        public MCACalibratorSettings()
            : base()
        {
            if (MCASettings == null)
            {
                MCASettings = new ObservableCollection<MCASetting>();
                MCASettings.Add(MCASetting.LargeLYSOSetting());
            }

            MCASettings.CollectionChanged += (o, s) =>
            {
                Debug.WriteLine(MCASettings.Count);
                this.Save();
            };

            this.Save();
        }

        private void ResetMCASettings(){
            MCASettings = new ObservableCollection<MCASetting>();
            MCASettings.Add(MCASetting.LargeLYSOSetting());
            MCASettings.CollectionChanged += (o, s) =>
            {
                Debug.WriteLine(MCASettings.Count);
                this.Save();
            };
        }

        public new void Reset()
        {
            base.Reset();
            ResetMCASettings();
            this.Save();
        }

        public override void Save()
        {
            base.Save();
            for (int i = 0; i < MCASettings.Count; i++)
            {
                Debug.WriteLine(MCASettings[i].Name);
            }
        }


        [UserScopedSetting]
        [SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Xml)]
        public ObservableCollection<MCASetting> MCASettings
        {
            get { return this["MCASettings"] as ObservableCollection<MCASetting>; }
            set { this["MCASettings"] = value as ObservableCollection<MCASetting>; }
        }
    }
}
