using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Transactions;
using System.Reflection;

namespace Bio.Core.IO.Genbank.Consumers;

/// <summary>
/// Provide specialized capabilities for cleaning up values in features.
/// This class is designed to provide a mechanism to clean up and process
/// values in the key/value pairs of GenBank features.This is useful
///    because in cases like::
///
///         /translation= "MED
///         YDPWNLRFQSKYKSRDA"
///
///    you'll otherwise end up with white space in it.
///
///    This cleaning needs to be done on a case by case basis since it is
///    impossible to interpret whether you should be concatenating everything
///    (as in translations), or combining things with spaces(as might be
///    the case with /notes).
///
///    >>> cleaner = FeatureValueCleaner(["translation"])
///    >>> cleaner
/// FeatureValueCleaner(['translation'])
///    >>> cleaner.clean_value("translation", "MED\nYDPWNLRFQSKYKSRDA")
///    'MEDYDPWNLRFQSKYKSRDA'
/// </summary>
public class FeatureValueCleaner
{
    public List<string> KeysToProcess { get; private set; } = new List<string>(){ "translation" };
    private List<string> toProcess = new List<string>();

    public FeatureValueCleaner()
    {
        toProcess = KeysToProcess;
    }

    public FeatureValueCleaner(List<string> keysToProcess)
    {
        toProcess = keysToProcess;
    }

    /// <summary>
    /// Clean the specified value and return it.
    ///
    /// If the value is not specified to be dealt with, the original value
    /// will be returned.
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public object CleanValue(string keyName, object? value)
    {
        if (toProcess.Contains(keyName))
        {
            var methodName = $"Clean{keyName}";
            var methodInfo = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                throw new InvalidOperationException($"No function to clean key: {keyName}");
            }
            value = methodInfo.Invoke(this, new[] { value });
        }
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        return value;
    }

    private string CleanTranslation(string value)
    {
        // Concatenate a translation value to one long protein string
        string[] translationParts = value.Split();
        return string.Join("", translationParts);
    }
}