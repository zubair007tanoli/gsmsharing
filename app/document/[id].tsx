import React, { useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  useColorScheme,
  Platform,
  ScrollView,
  Alert,
  Share,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import { useLocalSearchParams, router } from "expo-router";
import * as Haptics from "expo-haptics";
import Animated, { useAnimatedStyle, useSharedValue, withSpring } from "react-native-reanimated";
import Colors from "@/constants/colors";
import { useDocuments } from "@/context/DocumentsContext";

function InfoRow({ label, value }: { label: string; value: string }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  return (
    <View style={styles.infoRow}>
      <Text style={[styles.infoLabel, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>{label}</Text>
      <Text style={[styles.infoValue, { color: theme.text, fontFamily: "Inter_500Medium" }]}>{value}</Text>
    </View>
  );
}

function ActionButton({ icon, label, color, onPress }: { icon: string; label: string; color: string; onPress: () => void }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const scale = useSharedValue(1);
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));
  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.93); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
    >
      <Animated.View style={[styles.actionBtn, { backgroundColor: color + "18" }, style]}>
        <Ionicons name={icon as any} size={22} color={color} />
        <Text style={[styles.actionBtnLabel, { color, fontFamily: "Inter_500Medium" }]}>{label}</Text>
      </Animated.View>
    </Pressable>
  );
}

export default function DocumentScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { id } = useLocalSearchParams<{ id: string }>();
  const { documents, toggleStar, deleteDocument, renameDocument } = useDocuments();
  const [activeTab, setActiveTab] = useState<"preview" | "info">("preview");

  const doc = documents.find((d) => d.id === id);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  if (!doc) {
    return (
      <View style={[styles.container, { backgroundColor: theme.background, justifyContent: "center", alignItems: "center" }]}>
        <Ionicons name="document-outline" size={52} color={theme.textMuted} />
        <Text style={[styles.notFound, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
          Document not found
        </Text>
        <Pressable onPress={() => router.back()}>
          <Text style={[{ color: theme.primary, fontFamily: "Inter_500Medium", marginTop: 12 }]}>Go back</Text>
        </Pressable>
      </View>
    );
  }

  const typeColor =
    doc.type === "pdf" ? "#EF4444" : doc.type === "image" ? "#8B5CF6" : doc.type === "word" ? "#2563EB" : "#10B981";
  const typeIcon =
    doc.type === "pdf" ? "file-pdf-box" : doc.type === "image" ? "file-image" : doc.type === "word" ? "file-word" : "file-excel";

  const handleRename = () => {
    Alert.prompt(
      "Rename Document",
      "Enter a new name:",
      [
        { text: "Cancel" },
        { text: "Rename", onPress: (text) => text && renameDocument(doc.id, text) },
      ],
      "plain-text",
      doc.name
    );
  };

  const handleDelete = () => {
    Alert.alert("Delete Document", `Are you sure you want to delete "${doc.name}"?`, [
      { text: "Cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: async () => {
          await deleteDocument(doc.id);
          router.back();
        },
      },
    ]);
  };

  const handleShare = async () => {
    try {
      await Share.share({ message: `Document: ${doc.name} (${doc.size}, ${doc.pages} pages)` });
    } catch (e) {}
  };

  const formatDate = (ts: number) => {
    return new Date(ts).toLocaleDateString("en-US", { month: "long", day: "numeric", year: "numeric" });
  };

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      {/* Header */}
      <View style={[styles.header, { paddingTop: topPad + 10, backgroundColor: theme.surface, borderBottomColor: theme.border }]}>
        <Pressable style={styles.backBtn} onPress={() => router.back()}>
          <Ionicons name="chevron-back" size={24} color={theme.primary} />
        </Pressable>
        <Text style={[styles.headerTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]} numberOfLines={1}>
          {doc.name}
        </Text>
        <View style={styles.headerActions}>
          <Pressable
            onPress={() => { toggleStar(doc.id); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
            style={styles.headerBtn}
          >
            <Ionicons name={doc.starred ? "star" : "star-outline"} size={22} color={doc.starred ? "#F59E0B" : theme.textMuted} />
          </Pressable>
          <Pressable
            onPress={() => {
              Alert.alert("Options", "", [
                { text: "Rename", onPress: handleRename },
                { text: "Share", onPress: handleShare },
                { text: "Delete", style: "destructive", onPress: handleDelete },
                { text: "Cancel", style: "cancel" },
              ]);
            }}
            style={styles.headerBtn}
          >
            <Ionicons name="ellipsis-horizontal" size={22} color={theme.textMuted} />
          </Pressable>
        </View>
      </View>

      {/* Tabs */}
      <View style={[styles.tabs, { backgroundColor: theme.surface, borderBottomColor: theme.border }]}>
        {(["preview", "info"] as const).map((tab) => (
          <Pressable
            key={tab}
            style={[styles.tab, activeTab === tab && { borderBottomColor: theme.primary, borderBottomWidth: 2 }]}
            onPress={() => setActiveTab(tab)}
          >
            <Text style={[
              styles.tabText,
              { color: activeTab === tab ? theme.primary : theme.textMuted, fontFamily: "Inter_500Medium" }
            ]}>
              {tab === "preview" ? "Preview" : "Info"}
            </Text>
          </Pressable>
        ))}
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 30 }}
      >
        {activeTab === "preview" ? (
          <View style={styles.previewContent}>
            {/* Document Preview Mock */}
            <View style={[styles.docPreview, { backgroundColor: theme.card }]}>
              <View style={styles.docPreviewInner}>
                <MaterialCommunityIcons name={typeIcon as any} size={80} color={typeColor} />
                <Text style={[styles.docPreviewName, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
                  {doc.name}
                </Text>
                <Text style={[styles.docPreviewMeta, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                  {doc.pages} {doc.pages === 1 ? "page" : "pages"} · {doc.size}
                </Text>
              </View>

              {/* Mock Pages */}
              {Array.from({ length: Math.min(doc.pages, 3) }).map((_, i) => (
                <View key={i} style={[styles.mockPage, { backgroundColor: "#fff", borderColor: theme.border }]}>
                  <View style={[styles.mockPageLine, { backgroundColor: theme.surfaceSecondary, width: "90%" }]} />
                  <View style={[styles.mockPageLine, { backgroundColor: theme.surfaceSecondary, width: "75%" }]} />
                  <View style={[styles.mockPageLine, { backgroundColor: theme.surfaceSecondary, width: "85%" }]} />
                  <View style={[styles.mockPageLine, { backgroundColor: theme.surfaceSecondary, width: "60%" }]} />
                  <View style={[styles.mockPageLine, { backgroundColor: theme.surfaceSecondary, width: "80%" }]} />
                  <View style={styles.mockPageNum}>
                    <Text style={[styles.mockPageNumText, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                      Page {i + 1}
                    </Text>
                  </View>
                </View>
              ))}
              {doc.pages > 3 && (
                <Text style={[styles.morePages, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                  + {doc.pages - 3} more pages
                </Text>
              )}
            </View>

            {/* Quick Actions */}
            <Text style={[styles.actionsTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
              Quick Actions
            </Text>
            <View style={styles.actionsGrid}>
              <ActionButton icon="share-outline" label="Share" color="#2563EB" onPress={handleShare} />
              <ActionButton icon="create-outline" label="Edit" color="#8B5CF6" onPress={() => Alert.alert("Edit", "Open in editor")} />
              <ActionButton icon="contract-outline" label="Compress" color="#10B981" onPress={() => Alert.alert("Compress", "Reducing file size...")} />
              <ActionButton icon="lock-closed-outline" label="Protect" color="#EF4444" onPress={() => Alert.alert("Protect", "Add password protection")} />
              <ActionButton icon="document-text-outline" label="OCR" color="#06B6D4" onPress={() => Alert.alert("OCR", "Extracting text...")} />
              <ActionButton icon="trash-outline" label="Delete" color="#EF4444" onPress={handleDelete} />
            </View>
          </View>
        ) : (
          <View style={styles.infoContent}>
            <View style={[styles.infoCard, { backgroundColor: theme.card }]}>
              <InfoRow label="File Name" value={doc.name} />
              <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
              <InfoRow label="File Type" value={doc.type.toUpperCase()} />
              <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
              <InfoRow label="File Size" value={doc.size} />
              <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
              <InfoRow label="Pages" value={`${doc.pages}`} />
              <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
              <InfoRow label="Created" value={formatDate(doc.createdAt)} />
              <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
              <InfoRow label="Modified" value={formatDate(doc.updatedAt)} />
              <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
              <InfoRow label="Starred" value={doc.starred ? "Yes" : "No"} />
              {doc.tags.length > 0 && (
                <>
                  <View style={[styles.infoSep, { backgroundColor: theme.border }]} />
                  <InfoRow label="Tags" value={doc.tags.join(", ")} />
                </>
              )}
            </View>
          </View>
        )}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingBottom: 12,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  backBtn: { padding: 4, marginRight: 4 },
  headerTitle: { flex: 1, fontSize: 16, marginHorizontal: 8 },
  headerActions: { flexDirection: "row", gap: 4 },
  headerBtn: { padding: 8 },
  tabs: { flexDirection: "row", borderBottomWidth: StyleSheet.hairlineWidth },
  tab: { flex: 1, paddingVertical: 14, alignItems: "center" },
  tabText: { fontSize: 14 },
  previewContent: { padding: 16 },
  docPreview: { borderRadius: 20, padding: 20, marginBottom: 24, alignItems: "center", gap: 16 },
  docPreviewInner: { alignItems: "center", gap: 8, paddingVertical: 16 },
  docPreviewName: { fontSize: 16, textAlign: "center" },
  docPreviewMeta: { fontSize: 13 },
  mockPage: { width: "100%", borderRadius: 10, borderWidth: 1, padding: 16, gap: 8 },
  mockPageLine: { height: 8, borderRadius: 4 },
  mockPageNum: { alignItems: "flex-end", marginTop: 8 },
  mockPageNumText: { fontSize: 11 },
  morePages: { fontSize: 13, paddingVertical: 8 },
  actionsTitle: { fontSize: 17, marginBottom: 12 },
  actionsGrid: { flexDirection: "row", flexWrap: "wrap", gap: 10 },
  actionBtn: { borderRadius: 14, paddingHorizontal: 16, paddingVertical: 12, alignItems: "center", gap: 6, minWidth: 80 },
  actionBtnLabel: { fontSize: 12 },
  infoContent: { padding: 16 },
  infoCard: { borderRadius: 18, overflow: "hidden" },
  infoRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", padding: 14 },
  infoLabel: { fontSize: 14 },
  infoValue: { fontSize: 14, maxWidth: "55%", textAlign: "right" },
  infoSep: { height: StyleSheet.hairlineWidth, marginHorizontal: 14 },
  notFound: { fontSize: 16, marginTop: 12 },
});
