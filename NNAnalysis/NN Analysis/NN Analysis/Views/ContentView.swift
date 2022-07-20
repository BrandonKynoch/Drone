//
//  ContentView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/19.
//

import SwiftUI

struct ContentView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    @State var tabIndex: Int = 0
    
    var body: some View {
        ZStack {
            BackgroundView(viewMode: .dark)
        
            VStack {
                Spacer().frame(height: ELEMENT_SPACING)
                
                Picker(selection: $tabIndex, label: Text("")) {
                    Text("Neural")
                        .modifier(BodyTextModifier())
                        .tag(0)
                    
                    Text("Fitness")
                        .modifier(BodyTextModifier())
                        .tag(1)
                }
                .pickerStyle(SegmentedPickerStyle())
                
                switch tabIndex {
                case 0:
                    NNGroupView(nnGroupFolder: dataHandler.currentViewingTrainingFolder!)
                case 1:
                    FitnessView()
                default:
                    Spacer()
                }
            }
        }
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
