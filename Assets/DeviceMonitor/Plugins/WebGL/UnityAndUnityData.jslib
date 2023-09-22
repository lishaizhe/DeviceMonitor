mergeInto(LibraryManager.library,{
    UnityDataSend:function(type,item)
    {
        UnityData(UTF8ToString(type),UTF8ToString(item));
    },
    UnityDataRecv:function(type)
    {
        var returnStr = getUnityData(UTF8ToString(type));
        if(typeof returnStr === "string")
        {
            var bufferSize = lengthBytesUTF8(returnStr)+1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(returnStr, buffer, bufferSize);
            return buffer;
        }
        else
        {
            return "";
        }
    },
    SetWindowMax:function(isFullscreen)
    {
        SetWindowFullscreen(isFullscreen)
    },
});