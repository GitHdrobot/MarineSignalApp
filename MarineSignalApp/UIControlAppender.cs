

using DevExpress.Xpo.Logger;
using DevExpress.XtraEditors;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace JerryMouse
{

    class UIControlAppender : AppenderSkeleton
    {
     
        public string FormName { get; set; }
        public string UIControlLogName { get; set; }

        private ListBoxControl mUIControlLog;
        public ListBoxControl AppenderUIControl
        {
            set
            {
                mUIControlLog = value;
            }
            get
            {
                return mUIControlLog;
            }
        }

        public UIControlAppender()
            : base()
        {

        }

        public UIControlAppender(ListBoxControl UIControlLog)
            : base()
        {
            this.mUIControlLog = UIControlLog;
        }
        private delegate void UpdateControlDelegate(LoggingEvent loggingEvent);

        private void UpdateControl(LoggingEvent loggingEvent)
        {
            // ...
            mUIControlLog.Text = loggingEvent.LoggerName;

        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (mUIControlLog == null)
            {
                if (String.IsNullOrEmpty(FormName) ||
                    String.IsNullOrEmpty(UIControlLogName))
                    return;

                Form form = Application.OpenForms[FormName];
                if (form == null)
                    return;

                mUIControlLog = (ListBoxControl)FindControlRecursive(form, UIControlLogName);
                if (mUIControlLog == null)
                    return;

                form.FormClosing += (s, e) => mUIControlLog = null;
            }
            mUIControlLog.Invoke((MethodInvoker)delegate
            {
                mUIControlLog.Text = loggingEvent.RenderedMessage + Environment.NewLine;
            });

        }

        private Control FindControlRecursive(Control root, string textBoxName)
        {
            if (root.Name == textBoxName) return root;
            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, textBoxName);
                if (t != null) return t;
            }
            return null;
        }

    }
}
