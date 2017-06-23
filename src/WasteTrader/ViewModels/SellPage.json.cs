using Starcounter;
using WasteTrader.Database;
using Simplified.Ring3;
using System.Linq;
using WasteTrader.Measurements;
using System;
using System.Collections.Generic;
using WasteTrader.Helpers;
using WasteTrader.MathUtils;

namespace WasteTrader.ViewModels
{
    partial class SellPage : Json
    {
        static SellPage()
        {
            DefaultTemplate.Waste.LatitudeDD.Value.InstanceType = typeof(double);
            DefaultTemplate.Waste.LongitudeDD.Value.InstanceType = typeof(double);
        }

        protected static Tuple<int, string>[] MassUnits = UnitsToTuples(Mass.Units);
        protected static Tuple<int, string>[] LengthUnits = UnitsToTuples(Length.Units);
        protected static Tuple<int, string>[] AreaUnits = UnitsToTuples(Area.Units);
        protected static Tuple<int, string>[] VolumeUnits = UnitsToTuples(Volume.Units);

        protected static Tuple<int, string>[] UnitsToTuples(IDictionary<int, Unit> dictionary)
        {
            return dictionary.Select(kvp => new Tuple<int, string>(kvp.Key + kvp.Value.Offset, kvp.Value.Text)).ToArray();
        }

        void Handle(Input.UnitSort action)
        {
            UnitSuffixes.Clear();
            switch ((UnitType)action.Value)
            {
                case UnitType.Mass:
                    AddUnitSuffixes(MassUnits);
                    break;
                case UnitType.Length:
                    AddUnitSuffixes(LengthUnits);
                    break;
                case UnitType.Area:
                    AddUnitSuffixes(AreaUnits);
                    break;
                case UnitType.Volume:
                    AddUnitSuffixes(VolumeUnits);
                    break;
            }
        }

        protected void AddUnitSuffixes(Tuple<int, string>[] options)
        {
            foreach (Tuple<int, string> unit in options)
            {
                var option = UnitSuffixes.Add();
                option.Label = unit.Item2;
                option.PrefixPower = unit.Item1;
            }
        }

        public bool ValidInput => 
            this.Waste.Description.IsValid &&
            this.Waste.Title.IsValid &&
            this.Waste.Price.IsValid;

        void Handle(Input.SubmitTrigger action)
        {
            if (ValidInput)
            {
                this.CreateWaste();
                this.ClearViewModel();
                this.SubmitMessage = "Annonsen �r nu inlagd";
            }
            else
            {
                this.SubmitMessage = "Du har inte fyllt i alla f�lt riktigt";
            }
        }

        private void CreateWaste()
        {
            Db.Transact(() =>
            {
                var location = new NoDBLocation(this.Waste.LongitudeDD.Value, this.Waste.LatitudeDD.Value);
                SellWaste sellWaste = new SellWaste(location)
                {
                    Title = this.Waste.Title.Value,
                    Description = this.Waste.Description.Value,
                    Quantity = this.Waste.Quantity.Value,
                    Price = this.Waste.Price.Value,
                    Unit = (UnitType)this.Waste.Unit.Value,
                    User = SystemUser.GetCurrentSystemUser()
                };
            });
        }

        private void ClearViewModel()
        {
            this.Waste.Description.Value = "";
            this.Waste.Quantity.Value = 0;
            this.Waste.Unit.Value = 0;
            this.Waste.Title.Value = "";
            this.Waste.Price.Value = 0;
        }

        [SellPage_json.Waste.Title]
        partial class WasteTitle : Json
        {
            private const int MinLength = 3;
            private const int MaxLength = 100;
            private string InvalidTitleWarning = $"L�senordet m�ste vara mellan {MinLength} och {MaxLength} tecken l�ngt";

            void Handle(Input.Value action)
            {
                this.IsValid = new StringValidation(action.Value)
                    .ValidateLength(MinLength, MaxLength)
                    .IsValid;

                this.Message = this.IsValid ? string.Empty : InvalidTitleWarning;
            }
        }

        [SellPage_json.Waste.Description]
        partial class WasteDescription : Json
        {
            private const int MinLength = 20;
            private const int MaxLength = 2000;
            private string InvalidDescriptionWarning = $"Beskrivningen m�ste vara mellan {MinLength} och {MaxLength} tecken l�ngt";

            void Handle(Input.Value action)
            {
                this.IsValid = new StringValidation(action.Value)
                    .ValidateLength(MinLength, MaxLength)
                    .IsValid;

                this.Message = this.IsValid ? string.Empty : InvalidDescriptionWarning;
            }
        }

        [SellPage_json.Waste.Price]
        partial class WastePrice : Json
        {
            private string InvalidPriceWarning = "Priset m�ste vara h�gre �n noll";

            void Handle(Input.Value action)
            {
                this.IsValid = new NumberValidation(action.Value)
                    .IsMoreThanZero()
                    .IsValid;

                this.Message = this.IsValid ? string.Empty : InvalidPriceWarning;
            }
        }
    }
}
