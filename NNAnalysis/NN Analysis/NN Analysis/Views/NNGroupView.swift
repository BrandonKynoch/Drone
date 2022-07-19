//
//  NNGraphView.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import SwiftUI

//https://blog.logrocket.com/building-custom-charts-swiftui/

struct NNGroupView: View {
    @EnvironmentObject var analysisHandler: AnalysisHandler
    
    let nnGroupFolder: NNGroupFolder
    @ObservedObject var nnGroup: NNGroup
    @State var selectedNN: String
    
    init (nnGroupFolder: NNGroupFolder) {
        self.nnGroupFolder = nnGroupFolder
        self.nnGroup = nnGroupFolder.nng!
        if nnGroupFolder.nng!.nnNames.count > 0 {
            if let selected = AnalysisHandler.singleton.selectedNNName {
                self.selectedNN = selected
            } else {
                self.selectedNN = nnGroupFolder.nng!.nnNames[0]
                AnalysisHandler.singleton.selectedNNName = nnGroupFolder.nng!.nnNames[0]
            }
            nnGroup.SetCurrentViewingNNFromName(fileName: selectedNN)
        } else {
            self.selectedNN = "NULL"
        }
    }
    
    var body: some View {
        ZStack {
            BackgroundView(viewMode: .dark)
        
            if nnGroup.nns.count > 0 {
                VStack {
                    HStack {
                        Text("\(nnGroupFolder.epochFolder?.folder.lastPathComponent ?? "")\t\(nnGroup.folder.lastPathComponent)")
                            .modifier(TitleTextModifier())
                            .foregroundColor(COL_TEXT)
                        Spacer()
                    }
                    
                    Spacer().frame(height: 10)
                    
                    HStack {
                        Text("Fitness: \(nnGroup.meta?.fitness ?? -1)")
                            .modifier(BodyTextModifier())
                            .foregroundColor(COL_TEXT)
                        Spacer()
                    }
                    
                    Spacer().frame(height: 10)
                    
                    NNGraph(nnGroup: nnGroup)
                    
                    Spacer().frame(height: 10)
                    
                    ScrollView {
                        VStack {
                            CustomStringPickerView(rowHeight: 40, selected: .init(get: { return selectedNN }, set: { val in
                                selectedNN = val
                                nnGroup.SetCurrentViewingNNFromName(fileName: val)
                                AnalysisHandler.singleton.selectedNNName = val
                            }), selectionOptions: nnGroup.nnNames, canEdit: .constant(true))
                            
                            if nnGroup.currentViewingNN != nil {
                                Spacer().frame(height: ELEMENT_SPACING)
                                
                                HStack {
                                    Text("Opacity Scaler: ")
                                        .foregroundColor(COL_TEXT)
                                        .modifier(BodyTextModifier())
                                    Spacer().frame(width: ELEMENT_SPACING)
                                    CustomSliderView(initialVal: analysisHandler.weightOpacityScaler, minVal: 0.1, maxVal: 3, onValueChange: { val in
                                        analysisHandler.weightOpacityScaler = val
                                    })
                                }
                            }
                            
                            Spacer()
                        }
                    }
                    .frame(height: 300)
                }
                .padding()
            } else {
                VStack {
                    Spacer()
                    HStack {
                        Text(nnGroup.folder.lastPathComponent)
                            .modifier(TitleTextModifier())
                            .foregroundColor(COL_ACC0)
                        Spacer()
                    }
                    Spacer()
                }
                .padding()
            }
        }
    }
}

//struct NNGroupView_Previews: PreviewProvider {
//    static var previews: some View {
//        NNGroupView()
//    }
//}
