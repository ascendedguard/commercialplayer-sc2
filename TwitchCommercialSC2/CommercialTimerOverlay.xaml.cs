using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TwitchCommercialSC2
{
    using System.Globalization;

    /// <summary>
    /// Interaction logic for CommercialTimerOverlay.xaml
    /// </summary>
    public partial class CommercialTimerOverlay
    {
        private int delay;

        private int secondsCommercials;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommercialTimerOverlay"/> class.
        /// </summary>
        public CommercialTimerOverlay()
        {
            this.InitializeComponent();

            this.Top = 0;
            this.Left = 0;
        }

        private System.Timers.Timer timer = new System.Timers.Timer(1000);

        /// <summary> Initializes a new instance of the <see cref="CommercialTimerOverlay"/> class. </summary>
        /// <param name="delay"> The delay. </param>
        /// <param name="secondsCommercials"> The number of seconds in commercials playing. </param>
        public CommercialTimerOverlay(int delay, int secondsCommercials) : this()
        {
            this.delay = delay;
            this.secondsCommercials = secondsCommercials;

            this.timer.AutoReset = true;
            this.timer.Elapsed += this.timer_Elapsed;

            if (this.delay > 0)
            {
                this.txtHeader.Text = "Delay for:";
                this.txtTimeRemaining.Text = delay.ToString();
            }
            else
            {
                this.txtHeader.Text = "Commercials playing:";
                this.txtTimeRemaining.Text = secondsCommercials.ToString();
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            this.timer.Start();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)this.DecrementTime);
        }

        private void DecrementTime()
        {
            if (this.delay > 0)
            {
                this.delay--;

                if (this.delay == 0)
                {
                    this.txtHeader.Text = "Commercials playing:";
                    this.txtTimeRemaining.Text = this.secondsCommercials.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    this.txtTimeRemaining.Text = this.delay.ToString(CultureInfo.InvariantCulture);
                }

                return;
            }

            this.secondsCommercials--;
            this.txtTimeRemaining.Text = this.secondsCommercials.ToString(CultureInfo.InvariantCulture);

            if (this.secondsCommercials == 0)
            {
                this.Close();
            }
        }
    }
}
