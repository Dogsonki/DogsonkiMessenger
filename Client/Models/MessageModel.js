import React from 'react'
import { Text, View, StyleSheet,ImageBackground } from 'react-native'

export class MessageModel
{
    constructor(msg,user)
    {
        this.msg = msg;
        this.user= user;
    }
}

export const Message = (model) =>
{
    return (
        <View style={styles.messagecontainer}>
            <ImageBackground style={styles.avatar} source={require('../assets/tempavatar.jpg')} /> 
            <View>
            <Text style={styles.messageuser}>{model.model.user} </Text>
                 <Text style={styles.message}> {model.model.msg} </Text>
            </View>         
            </View>
    );
}

const styles = StyleSheet.create({
    avatar:{
        width: 50,
        height: 50,
        borderRadius: 150 / 2,
        overflow: "hidden",
        borderWidth: 1,
    },
    messagecontainer:{
        flexDirection:'row',
    },
    messageuser:
    {
        left:25,
        fontSize:16
    },
    message:{
        left:10,
    }

})