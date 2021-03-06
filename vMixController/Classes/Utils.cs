﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Widgets;

namespace vMixController.Classes
{
    public static class Utils
    {
        static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static ObservableCollection<vMixControl> LoadController(string fileName, IList<vMixFunctionReference> functions, out MainWindowSettings windowSettings)
        {
            var _controls = new ObservableCollection<vMixControl>();
            using (var stream = File.OpenRead(fileName))
            {
                _logger.Info("Controller loading.");
                var reader = XmlReader.Create(stream);
                {
                    reader.ReadStartElement();
                    reader.ReadStartElement();
                    _logger.Info("Loading widgets.");
                    XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControl>));
                    var collection = (ObservableCollection<vMixControl>)s.Deserialize(reader);
                    foreach (var item in collection)
                    {
                        _controls.Add(item);
                        if (functions != null && item is vMixControlButton)
                        {
                            var btn = item as vMixControlButton;
                            foreach (var command in btn.Commands)
                            {
                                var newFunction = functions.Where(x => x.Function == command.Action.Function).FirstOrDefault();
                                if (newFunction != null)
                                    command.Action = newFunction;
                            }
                        }
                    }
                    reader.ReadEndElement();
                    reader.ReadStartElement();

                    _logger.Info("Loading window settings.");
                    s = new XmlSerializer(typeof(MainWindowSettings));

                    var settings = (MainWindowSettings)s.Deserialize(reader);
                    windowSettings = settings;

                    reader.ReadEndElement();
                    reader.ReadEndElement();

                    _logger.Info("Configuring API.");
                }

            }
            return _controls;
        }

        public static void SaveController(string fileName, ObservableCollection<vMixControl> _controls, MainWindowSettings _windowSettings)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                _logger.Info("Saving controller.");
                var writer = XmlWriter.Create(stream);
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Root");
                    writer.WriteStartElement("Controls");
                    XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControl>));
                    _logger.Info("Writing widgets.");
                    s.Serialize(writer, _controls);
                    writer.WriteEndElement();
                    writer.WriteStartElement("WindowSettings");
                    s = new XmlSerializer(typeof(MainWindowSettings));
                    _logger.Info("Writing window settings.");
                    s.Serialize(writer, _windowSettings);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }
            }
        }
    }
}
